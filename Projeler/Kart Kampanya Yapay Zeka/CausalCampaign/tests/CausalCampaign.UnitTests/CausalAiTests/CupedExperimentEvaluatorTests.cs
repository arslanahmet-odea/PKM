using CausalCampaign.CausalAi.Analytics;
using CausalCampaign.Domain.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.CausalAiTests;

[TestClass]
public class CupedExperimentEvaluatorTests
{
    [TestMethod]
    public void EvaluateCampaignIncrementalRoi_WithPreExperimentCovariate_ReducesVarianceAndExceedsKpi3Target()
    {
        // Arrange
        var evaluator = new CupedExperimentEvaluator();
        decimal marginRate = 0.0260m; // 2.60% interchange/interest margin

        // Stratified RCT (Treatment vs Control_Holdout) observations:
        // In the Treatment group, ~28,000 TRY causal incremental spend per customer (28,000 * 0.026 = 728 TRY gross contribution) and 480 TRY reward cost
        // Incremental ROI = 728 / 480 = 1.5167x (>= 1.35x KPI-3 target)
        var observations = new List<(decimal PreExperimentSpend, decimal PostExperimentSpend, decimal RewardCost, bool IsTreatment)>
        {
            // Treatment Group
            (15000m, 43500m, 480m, true),
            (20000m, 48200m, 480m, true),
            (25000m, 53800m, 480m, true),
            (30000m, 58100m, 480m, true),
            (35000m, 63400m, 480m, true),
            (40000m, 68000m, 480m, true),

            // Control (Hold-out) Group (Organic spend only, reward=0)
            (15000m, 15400m, 0m, false),
            (20000m, 20300m, 0m, false),
            (25000m, 25500m, 0m, false),
            (30000m, 30200m, 0m, false),
            (35000m, 35400m, 0m, false),
            (40000m, 40100m, 0m, false)
        };

        // Act
        var result = evaluator.EvaluateCampaignIncrementalRoi(1, marginRate, observations);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Value.VarianceReductionPercentage > 40.00m, "CUPED variance reduction percentage must exceed 40%.");
        Assert.IsTrue(
            result.Value.MeetsKpi3Threshold,
            $"KPI-3 Incremental ROI target ({KpiThresholds.MinIncrementalRoi}x) was not met. Actual: {result.Value.IncrementalRoi}x");
        Assert.IsTrue(result.Value.TotalIncrementalNetProfit > 0m);
    }
}
