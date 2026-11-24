namespace CausalCampaign.Domain.Entities;

/// <summary>
/// Read-only entity representing the dbo.vw_CasualCampaign_CustomerCausalFeatures SQL view (Inflation-Adjusted Feature Store).
/// </summary>
public class CustomerCausalFeatureView
{
    public int CustomerId { get; set; }
    public string CustomerExternalNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PrestigeSegment { get; set; } = "Standard";
    public decimal TotalCreditLimit { get; set; }
    public decimal AvailableCreditLimit { get; set; }
    public decimal AvailableLimitRatio { get; set; }
    public decimal MisMonthlyNetProfit { get; set; }
    public decimal HistoricalBaselineSpend { get; set; }
    public decimal RealBaselineSpendIndex { get; set; }
    public string SpendElasticityPersona { get; set; } = "Persuadable";
    public bool IsDormant { get; set; }
    public int DaysSinceLastTx { get; set; }
    public bool HasMarketingConsent { get; set; }
    public string PreferredRewardType { get; set; } = "RewardPoints";
    public int BehavioralClusterId { get; set; }
    public int ValidTxCount { get; set; }
    public decimal RecentNominalSpend { get; set; }
    public int DistinctSectorCount { get; set; }
    public decimal PortfolioRealSpendQuantile { get; set; }
    public int SpendDecile { get; set; }
}
