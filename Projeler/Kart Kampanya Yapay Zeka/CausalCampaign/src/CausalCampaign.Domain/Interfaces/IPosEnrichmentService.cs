using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;

namespace CausalCampaign.Domain.Interfaces;

/// <summary>
/// Service contract that enriches ISO 8583 DE-18 (MCC) and DE-43 (Merchant Name) fields via a 3-layer hybrid architecture.
/// </summary>
public interface IPosEnrichmentService
{
    /// <summary>
    /// Resolves raw POS merchant text and MCC code into brand, sector, and cash-like/excluded MCC status.
    /// </summary>
    Task<Result<PosEnrichmentResultDto>> EnrichTransactionMerchantAsync(
        PosEnrichmentRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates KPI-1 Macro F1-Score performance on a ground-truth labeled dataset.
    /// </summary>
    Result<PosEvaluationMetricsDto> EvaluateMacroF1Score(
        IReadOnlyList<(string RawMerchant_DE43, string RawMcc_DE18, string ExpectedSector)> labeledSamples);
}
