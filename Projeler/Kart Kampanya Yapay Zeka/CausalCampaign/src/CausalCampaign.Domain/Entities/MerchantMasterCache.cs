namespace CausalCampaign.Domain.Entities;

/// <summary>
/// Entity representing the MERCHANT_MASTER_CACHE table for 3-layer POS semantic classification caching.
/// </summary>
public class MerchantMasterCache
{
    public string RawMerchantKey { get; set; } = string.Empty;
    public string RawMcc_DE18 { get; set; } = string.Empty;
    public string CleanedBrandName { get; set; } = string.Empty;
    public string ResolvedSector { get; set; } = string.Empty;
    public string? SubMerchantProvider { get; set; }
    public byte ClassificationLayer { get; set; } = 1;
    public decimal ConfidenceScore { get; set; } = 1.0000m;
    public bool IsCashLikeOrExcludedMcc { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
