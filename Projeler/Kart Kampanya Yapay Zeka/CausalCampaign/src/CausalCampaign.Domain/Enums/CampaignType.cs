namespace CausalCampaign.Domain.Enums;

/// <summary>
/// Specifies the strategic objective type of a campaign.
/// </summary>
public enum CampaignType
{
    /// <summary>Total spend increase for active cardholders (Upsell).</summary>
    TotalSpendUpsell = 1,

    /// <summary>Guiding cardholders to a new merchant sector (Cross-Sell).</summary>
    SectoralCrossSell = 2,

    /// <summary>Reactivating dormant cards with no transactions in the last 90+ days.</summary>
    DormantReactivation = 3
}
