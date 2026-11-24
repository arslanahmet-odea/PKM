using CausalCampaign.Domain.Enums;

namespace CausalCampaign.Domain.DTOs;

/// <summary>
/// ISO 8583 DE-18 (MCC) and DE-43 (Merchant Name/Location) enrichment request.
/// </summary>
public sealed record PosEnrichmentRequestDto(
    string RawMerchantName_DE43,
    string RawMcc_DE18);

/// <summary>
/// Output record of the 3-layer POS transaction enrichment engine.
/// </summary>
public sealed record PosEnrichmentResultDto(
    string RawMerchantKey,
    string RawMcc_DE18,
    string CleanedBrandName,
    string ResolvedSector,
    string? SubMerchantProvider,
    PosClassificationLayer ClassificationLayer,
    decimal ConfidenceScore,
    bool IsCashLikeOrExcludedMcc,
    bool ServedFromCache,
    double ProcessingTimeMs);

/// <summary>
/// Ground-truth POS classification performance metrics for KPI-1 verification.
/// </summary>
public sealed record PosEvaluationMetricsDto(
    int TotalSampleCount,
    int CorrectPredictionCount,
    decimal MacroPrecision,
    decimal MacroRecall,
    decimal MacroF1Score,
    bool MeetsKpi1Threshold);
