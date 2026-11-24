using CausalCampaign.Domain.Enums;

namespace CausalCampaign.PosEnrichment.Layers;

/// <summary>
/// Represents an intermediate classification match produced by one of the POS enrichment layers.
/// </summary>
public sealed record LayerClassificationMatch(
    string CleanedBrandName,
    string ResolvedSector,
    string? SubMerchantProvider,
    PosClassificationLayer Layer,
    decimal ConfidenceScore,
    bool IsCashLikeOrExcludedMcc);

/// <summary>
/// Chain of Responsibility handler interface for the 3-layer POS classification pipeline.
/// </summary>
public interface IPosClassificationLayerHandler
{
    /// <summary>
    /// Attempts to classify the normalized merchant text and MCC code; returns null if no match is found at this layer.
    /// </summary>
    LayerClassificationMatch? TryClassify(string normalizedMerchantText, string rawMcc);
}
