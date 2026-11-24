using System.Diagnostics;
using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.Enums;
using CausalCampaign.Domain.Interfaces;
using CausalCampaign.PosEnrichment.Caching;
using CausalCampaign.PosEnrichment.Configuration;
using CausalCampaign.PosEnrichment.Layers;
using Microsoft.Extensions.Options;

namespace CausalCampaign.PosEnrichment.Services;

/// <summary>
/// Primary service that resolves ISO 8583 DE-18 (MCC) and DE-43 (Merchant Name/Location) fields
/// via L1/L2 Cache + 3-Layer (1: Brand Dictionary, 2: Payment Facilitator Regex, 3: ML.NET LightGBM) architecture.
/// </summary>
public sealed class PosEnrichmentService : IPosEnrichmentService
{
    private readonly PosMerchantCacheManager _cacheManager;
    private readonly DeterministicBrandDictionaryLayer _layer1Dictionary;
    private readonly PaymentFacilitatorRegexLayer _layer2PfRegex;
    private readonly LocalMlMerchantClassifierLayer _layer3MlClassifier;
    private readonly ICausalCampaignDbContext? _dbContext;
    private readonly PosEnrichmentOptions _options;

    public PosEnrichmentService(
        PosMerchantCacheManager cacheManager,
        DeterministicBrandDictionaryLayer layer1Dictionary,
        PaymentFacilitatorRegexLayer layer2PfRegex,
        LocalMlMerchantClassifierLayer layer3MlClassifier,
        IOptions<PosEnrichmentOptions>? options = null,
        ICausalCampaignDbContext? dbContext = null)
    {
        _cacheManager = cacheManager;
        _layer1Dictionary = layer1Dictionary;
        _layer2PfRegex = layer2PfRegex;
        _layer3MlClassifier = layer3MlClassifier;
        _options = options?.Value ?? new PosEnrichmentOptions();
        _dbContext = dbContext;
    }

    /// <summary>
    /// Resolves raw POS merchant text and MCC code into brand, sector, and excluded MCC status.
    /// </summary>
    public async Task<Result<PosEnrichmentResultDto>> EnrichTransactionMerchantAsync(
        PosEnrichmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.RawMerchantName_DE43))
        {
            return Result<PosEnrichmentResultDto>.Failure(DomainError.Validation(
                "POS_MERCHANT_NAME_REQUIRED",
                "ISO 8583 DE-43 merchant name cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(request.RawMcc_DE18))
        {
            return Result<PosEnrichmentResultDto>.Failure(DomainError.Validation(
                "POS_MCC_REQUIRED",
                "ISO 8583 DE-18 MCC code cannot be empty."));
        }

        var stopwatch = Stopwatch.StartNew();
        string normalizedKey = PosMerchantCacheManager.NormalizeMerchantKey(request.RawMerchantName_DE43);
        string trimmedMcc = request.RawMcc_DE18.Trim();

        // 0. L1 / L2 Cache Lookup (< 1 ms) keyed by (NormalizedMerchant, MCC)
        var cached = await _cacheManager.TryGetFromCacheAsync(normalizedKey, trimmedMcc, _dbContext, cancellationToken);
        if (cached is not null)
        {
            stopwatch.Stop();
            return Result<PosEnrichmentResultDto>.Success(new PosEnrichmentResultDto(
                RawMerchantKey: cached.RawMerchantKey,
                RawMcc_DE18: cached.RawMcc_DE18,
                CleanedBrandName: cached.CleanedBrandName,
                ResolvedSector: cached.ResolvedSector,
                SubMerchantProvider: cached.SubMerchantProvider,
                ClassificationLayer: (PosClassificationLayer)cached.ClassificationLayer,
                ConfidenceScore: cached.ConfidenceScore,
                IsCashLikeOrExcludedMcc: cached.IsCashLikeOrExcludedMcc || AntiGamingConstants.IsExcludedMcc(trimmedMcc),
                ServedFromCache: true,
                ProcessingTimeMs: stopwatch.Elapsed.TotalMilliseconds));
        }

        // 1..3 Classification Chain
        var match = ClassifyThroughThreeLayers(normalizedKey, trimmedMcc);

        var newCacheEntry = new MerchantMasterCache
        {
            RawMerchantKey = normalizedKey,
            RawMcc_DE18 = trimmedMcc,
            CleanedBrandName = match.CleanedBrandName,
            ResolvedSector = match.ResolvedSector,
            SubMerchantProvider = match.SubMerchantProvider,
            ClassificationLayer = (byte)match.Layer,
            ConfidenceScore = match.ConfidenceScore,
            IsCashLikeOrExcludedMcc = match.IsCashLikeOrExcludedMcc,
            UpdatedAt = DateTime.UtcNow
        };

        await _cacheManager.StoreInCacheAsync(
            newCacheEntry,
            _dbContext,
            _options.PersistNewResolutionsToDatabaseCache,
            cancellationToken);

        stopwatch.Stop();
        return Result<PosEnrichmentResultDto>.Success(new PosEnrichmentResultDto(
            RawMerchantKey: normalizedKey,
            RawMcc_DE18: trimmedMcc,
            CleanedBrandName: match.CleanedBrandName,
            ResolvedSector: match.ResolvedSector,
            SubMerchantProvider: match.SubMerchantProvider,
            ClassificationLayer: match.Layer,
            ConfidenceScore: match.ConfidenceScore,
            IsCashLikeOrExcludedMcc: match.IsCashLikeOrExcludedMcc,
            ServedFromCache: false,
            ProcessingTimeMs: stopwatch.Elapsed.TotalMilliseconds));
    }

    /// <summary>
    /// Evaluates KPI-1 Macro F1-Score performance on a ground-truth labeled dataset.
    /// </summary>
    public Result<PosEvaluationMetricsDto> EvaluateMacroF1Score(
        IReadOnlyList<(string RawMerchant_DE43, string RawMcc_DE18, string ExpectedSector)> labeledSamples)
    {
        if (labeledSamples is null || labeledSamples.Count == 0)
        {
            return Result<PosEvaluationMetricsDto>.Failure(DomainError.Validation(
                "EMPTY_EVALUATION_DATASET",
                "At least 1 labeled sample is required for Macro F1-Score evaluation."));
        }

        var predictions = new List<(string Expected, string Actual)>(labeledSamples.Count);
        int correctCount = 0;

        foreach (var sample in labeledSamples)
        {
            string normalized = PosMerchantCacheManager.NormalizeMerchantKey(sample.RawMerchant_DE43 ?? string.Empty);
            string mcc = (sample.RawMcc_DE18 ?? string.Empty).Trim();
            var match = ClassifyThroughThreeLayers(normalized, mcc);
            predictions.Add((sample.ExpectedSector, match.ResolvedSector));

            if (string.Equals(sample.ExpectedSector, match.ResolvedSector, StringComparison.OrdinalIgnoreCase))
            {
                correctCount++;
            }
        }

        var distinctClasses = predictions
            .Select(p => p.Expected)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        decimal totalPrecision = 0m;
        decimal totalRecall = 0m;
        decimal totalF1 = 0m;

        foreach (string cls in distinctClasses)
        {
            int tp = predictions.Count(p =>
                string.Equals(p.Expected, cls, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.Actual, cls, StringComparison.OrdinalIgnoreCase));

            int fp = predictions.Count(p =>
                !string.Equals(p.Expected, cls, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.Actual, cls, StringComparison.OrdinalIgnoreCase));

            int fn = predictions.Count(p =>
                string.Equals(p.Expected, cls, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(p.Actual, cls, StringComparison.OrdinalIgnoreCase));

            decimal precision = (tp + fp) > 0 ? (decimal)tp / (tp + fp) : 0m;
            decimal recall = (tp + fn) > 0 ? (decimal)tp / (tp + fn) : 0m;
            decimal f1 = (precision + recall) > 0m
                ? (2m * precision * recall) / (precision + recall)
                : 0m;

            totalPrecision += precision;
            totalRecall += recall;
            totalF1 += f1;
        }

        int classCount = distinctClasses.Count;
        decimal macroPrecision = Math.Round(totalPrecision / classCount, 4);
        decimal macroRecall = Math.Round(totalRecall / classCount, 4);
        decimal macroF1 = Math.Round(totalF1 / classCount, 4);

        return Result<PosEvaluationMetricsDto>.Success(new PosEvaluationMetricsDto(
            TotalSampleCount: labeledSamples.Count,
            CorrectPredictionCount: correctCount,
            MacroPrecision: macroPrecision,
            MacroRecall: macroRecall,
            MacroF1Score: macroF1,
            MeetsKpi1Threshold: macroF1 >= KpiThresholds.MinPosClassificationMacroF1));
    }

    private LayerClassificationMatch ClassifyThroughThreeLayers(string normalizedMerchantText, string trimmedMcc)
    {
        return _layer1Dictionary.TryClassify(normalizedMerchantText, trimmedMcc)
               ?? _layer2PfRegex.TryClassify(normalizedMerchantText, trimmedMcc)
               ?? _layer3MlClassifier.TryClassify(normalizedMerchantText, trimmedMcc);
    }
}
