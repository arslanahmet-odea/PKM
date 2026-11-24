namespace CausalCampaign.PosEnrichment.Configuration;

/// <summary>
/// Configuration options for the 3-layer POS transaction enrichment engine.
/// </summary>
public sealed class PosEnrichmentOptions
{
    public const string SectionName = "PosEnrichment";

    /// <summary>
    /// Indicates whether newly resolved merchants should be automatically persisted to the L2 database cache (CasualCampaign_MERCHANT_MASTER_CACHE).
    /// </summary>
    public bool PersistNewResolutionsToDatabaseCache { get; set; } = true;

    /// <summary>
    /// Maximum number of unique merchant entries kept in the L1 in-memory cache.
    /// </summary>
    public int MaxInMemoryCacheEntries { get; set; } = 50_000;

    /// <summary>
    /// Minimum acceptable confidence threshold for the Layer 3 ML.NET classifier.
    /// </summary>
    public decimal MinimumMlConfidenceThreshold { get; set; } = 0.45m;
}
