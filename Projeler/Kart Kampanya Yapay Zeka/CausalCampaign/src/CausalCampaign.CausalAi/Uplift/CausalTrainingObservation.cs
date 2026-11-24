using Microsoft.ML.Data;

namespace CausalCampaign.CausalAi.Uplift;

/// <summary>
/// ML.NET input schema for Robinson R-Learner Stage 1 (Expected Organic Spend m_hat) and Stage 2 (Pseudo-Outcome tau_hat).
/// </summary>
public sealed class CausalCustomerInput
{
    public float RealBaselineSpendIndex { get; set; }
    public float AvailableLimitRatio { get; set; }
    public float MisMonthlyNetProfit { get; set; }
    public float ValidTxCount { get; set; }
    public float DistinctSectorCount { get; set; }
    public float PortfolioRealSpendQuantile { get; set; }
    public float IsGoldOrPrivateSegment { get; set; }
    public float Label { get; set; }
    public float Weight { get; set; } = 1.0f;
}

/// <summary>
/// ML.NET LightGBM regression score output schema.
/// </summary>
public sealed class RegressionScorePrediction
{
    [ColumnName("Score")]
    public float Score { get; set; }
}
