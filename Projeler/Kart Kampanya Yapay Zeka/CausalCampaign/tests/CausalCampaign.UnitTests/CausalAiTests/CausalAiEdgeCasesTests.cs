using CausalCampaign.CausalAi.Analytics;
using CausalCampaign.CausalAi.Bandit;
using CausalCampaign.CausalAi.Explainability;
using CausalCampaign.CausalAi.FeatureStore;
using CausalCampaign.CausalAi.Recommendation;
using CausalCampaign.CausalAi.Uplift;
using CausalCampaign.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.CausalAiTests;

[TestClass]
public class CausalAiEdgeCasesTests
{
    [TestMethod]
    public void CupedExperimentEvaluator_ReportsNegativeLiftForHarmfulCampaign_AndValidatesInputs()
    {
        var evaluator = new CupedExperimentEvaluator();

        // Validation failures
        Assert.IsTrue(evaluator.EvaluateCampaignIncrementalRoi(1, 0m, []).IsFailure);
        Assert.IsTrue(evaluator.EvaluateCampaignIncrementalRoi(1, 0.025m, [(100m, 100m, 10m, true)]).IsFailure);

        var negativeRewardObs = new List<(decimal, decimal, decimal, bool)>
        {
            (1000m, 1200m, -50m, true), (2000m, 2200m, 10m, true),
            (1000m, 1000m, 0m, false), (2000m, 2000m, 0m, false)
        };
        Assert.AreEqual("NEGATIVE_REWARD_COST", evaluator.EvaluateCampaignIncrementalRoi(1, 0.025m, negativeRewardObs).Error.Code);

        // Harmful ("Sleeping Dog") campaign where Treatment spends LESS than Control
        var harmfulObs = new List<(decimal, decimal, decimal, bool)>
        {
            (10000m, 8000m, 200m, true), (20000m, 17500m, 200m, true),
            (10000m, 10500m, 0m, false), (20000m, 20500m, 0m, false)
        };
        var harmfulResult = evaluator.EvaluateCampaignIncrementalRoi(1, 0.025m, harmfulObs);
        Assert.IsTrue(harmfulResult.IsSuccess);
        Assert.IsTrue(harmfulResult.Value.CupedIncrementalSpendPerCustomer < 0m, "Negative incremental lift must not be masked as 0.");
        Assert.IsTrue(harmfulResult.Value.TotalIncrementalNetProfit < 0m);
        Assert.IsFalse(harmfulResult.Value.MeetsKpi3Threshold);

        // Zero-variance pre-experiment spend (Var(X) == 0 -> theta == 0)
        var constantPreObs = new List<(decimal, decimal, decimal, bool)>
        {
            (10000m, 15000m, 100m, true), (10000m, 16000m, 100m, true),
            (10000m, 10000m, 0m, false), (10000m, 10500m, 0m, false)
        };
        var constResult = evaluator.EvaluateCampaignIncrementalRoi(1, 0.025m, constantPreObs);
        Assert.IsTrue(constResult.IsSuccess);
        Assert.AreEqual(0m, constResult.Value.ThetaCovariateWeight);
    }

    [TestMethod]
    public void InflationPsiDriftMonitor_HandlesMinimumSampleSizeFourAndConstantDistributions()
    {
        var monitor = new InflationPsiDriftMonitor();

        // Identical N=4 distributions -> PSI == 0
        decimal[] fourVals = [1000m, 2000m, 3000m, 4000m];
        var sameRes = monitor.CalculateMonthlyPsiDrift(202606, 202611, fourVals, fourVals, fourVals, fourVals);
        Assert.IsTrue(sameRes.IsSuccess);
        Assert.AreEqual(0.0000m, sameRes.Value.InflationAdjustedRealSpendPsi);
        Assert.IsTrue(sameRes.Value.IsDriftWithinSafeThreshold);

        // Zero-variance identical constant distributions -> PSI == 0
        decimal[] constVals = [5000m, 5000m, 5000m, 5000m];
        var constRes = monitor.CalculateMonthlyPsiDrift(202606, 202611, constVals, constVals, constVals, constVals);
        Assert.IsTrue(constRes.IsSuccess);
        Assert.AreEqual(0.0000m, constRes.Value.InflationAdjustedRealSpendPsi);

        // Constant reference vs completely shifted actual -> PSI alerts (> 0.15)
        decimal[] shiftedVals = [9000m, 9500m, 10000m, 11000m];
        var shiftedRes = monitor.CalculateMonthlyPsiDrift(202606, 202611, constVals, shiftedVals, constVals, shiftedVals);
        Assert.IsTrue(shiftedRes.IsSuccess);
        Assert.IsFalse(shiftedRes.Value.IsDriftWithinSafeThreshold);
    }

    [TestMethod]
    public void InflationAdjustedFeatureStore_DeflationHandlesNullIndicesAndInvalidMultipliers()
    {
        Assert.AreEqual(0m, InflationAdjustedFeatureStore.DeflateNominalToRealSpend(-100m, 202606, "Fuel", null));
        Assert.AreEqual(1234.56m, InflationAdjustedFeatureStore.DeflateNominalToRealSpend(1234.56m, 202606, "Fuel", null));

        var badIndices = new List<InflationIndexMonthly>
        {
            new() { YearMonth = 202606, SectorName = "General", DeflatorMultiplier = -0.5m }
        };
        Assert.AreEqual(1000.00m, InflationAdjustedFeatureStore.DeflateNominalToRealSpend(1000m, 202606, "Fuel", badIndices));
    }

    [TestMethod]
    public async Task RobinsonRLearner_ValidatesNullCampaign_UnifiesDormancyAndSupportsConcurrentCalls()
    {
        var engine = new RobinsonRLearnerUpliftEngine(
            new DormantCardReactivationModel(),
            new MatrixFactorizationSectorRecommender(),
            new ThompsonSamplingBanditService(),
            new XaiTurkishExplanationService());

        var campaign = new CampaignTemplate
        {
            CampaignId = 1,
            MaxRewardPerCustomer = 2000m,
            MinMisProfitThreshold = -250m,
            InterchangeMarginRate = 0.025m
        };

        var dormantByDaysCustomer = new CustomerCausalFeatureView
        {
            CustomerId = 2001,
            CustomerExternalNo = "CUST-2001",
            FullName = "Dormant By Days",
            PrestigeSegment = "Gold",
            AvailableCreditLimit = 50000m,
            AvailableLimitRatio = 0.80m,
            MisMonthlyNetProfit = 1500m,
            RealBaselineSpendIndex = 200m,
            IsDormant = false, // Flag false, but DaysSinceLastTx >= 90
            DaysSinceLastTx = 95,
            HasMarketingConsent = true
        };

        // Null campaign rejected
        Assert.IsTrue(engine.PredictPortfolioCausalUplift([dormantByDaysCustomer], null!).IsFailure);

        // Concurrent parallel scoring (verifies PredictionEngine disposal & thread safety)
        var tasks = Enumerable.Range(0, 8)
            .Select(_ => Task.Run(() => engine.PredictPortfolioCausalUplift([dormantByDaysCustomer], campaign)))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        foreach (var res in results)
        {
            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual("Dormant_Sleeper", res.Value[0].PredictedPersona);
            Assert.AreEqual(200.00m, res.Value[0].ExpectedOrganicSpend);
        }
    }
}
