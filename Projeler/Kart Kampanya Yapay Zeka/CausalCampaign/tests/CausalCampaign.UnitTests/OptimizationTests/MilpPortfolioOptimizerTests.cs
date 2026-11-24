using CausalCampaign.CausalAi.Bandit;
using CausalCampaign.CausalAi.Explainability;
using CausalCampaign.CausalAi.Recommendation;
using CausalCampaign.CausalAi.Uplift;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Optimization.Buckets;
using CausalCampaign.Optimization.Bundles;
using CausalCampaign.Optimization.Experiment;
using CausalCampaign.Optimization.Solvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.OptimizationTests;

[TestClass]
public class MilpPortfolioOptimizerTests
{
    [TestMethod]
    public void OptimizeCampaignPortfolio_RespectsBudgetLimitCreditLimitExcludesFreeRidersAndGuaranteesMonotonicity()
    {
        // Arrange
        var rLearner = new RobinsonRLearnerUpliftEngine(
            new DormantCardReactivationModel(),
            new MatrixFactorizationSectorRecommender(),
            new ThompsonSamplingBanditService(),
            new XaiTurkishExplanationService());

        var optimizer = new MilpPortfolioOptimizer(
            new MicroBucketCatalogService(),
            new EquivalentOfferBundleBuilder(),
            new StratifiedRctHoldoutSplitter());

        var campaign = new CampaignTemplate
        {
            CampaignId = 1,
            CampaignCode = "CMP-2026-UPSELL-01",
            TotalBudget = 5000.00m,
            ConsumedBudget = 0.00m,
            MaxRewardPerCustomer = 2500.00m,
            MinMisProfitThreshold = -250.00m,
            InterchangeMarginRate = 0.0280m
        };

        var customers = new List<CustomerCausalFeatureView>
        {
            // 1001: Gold Persuadable
            new()
            {
                CustomerId = 1001, CustomerExternalNo = "CUST-1001", FullName = "Ahmet Yilmaz", PrestigeSegment = "Gold",
                TotalCreditLimit = 150000m, AvailableCreditLimit = 110000m, AvailableLimitRatio = 0.7333m,
                MisMonthlyNetProfit = 4250m, HistoricalBaselineSpend = 24000m, RealBaselineSpendIndex = 25800m,
                SpendElasticityPersona = "Persuadable", HasMarketingConsent = true, ValidTxCount = 8, DistinctSectorCount = 4, PortfolioRealSpendQuantile = 0.65m, SpendDecile = 7
            },
            // 1002: Silver Persuadable
            new()
            {
                CustomerId = 1002, CustomerExternalNo = "CUST-1002", FullName = "Zeynep Kaya", PrestigeSegment = "Silver",
                TotalCreditLimit = 85000m, AvailableCreditLimit = 62000m, AvailableLimitRatio = 0.7294m,
                MisMonthlyNetProfit = 2150m, HistoricalBaselineSpend = 14500m, RealBaselineSpendIndex = 15400m,
                SpendElasticityPersona = "Persuadable", HasMarketingConsent = true, ValidTxCount = 6, DistinctSectorCount = 3, PortfolioRealSpendQuantile = 0.45m, SpendDecile = 5
            },
            // 1003: Private Free-Rider (Must be excluded)
            new()
            {
                CustomerId = 1003, CustomerExternalNo = "CUST-1003", FullName = "Mehmet Demir", PrestigeSegment = "Private",
                TotalCreditLimit = 300000m, AvailableCreditLimit = 210000m, AvailableLimitRatio = 0.7000m,
                MisMonthlyNetProfit = 8900m, HistoricalBaselineSpend = 65000m, RealBaselineSpendIndex = 68200m,
                SpendElasticityPersona = "FreeRider_SureThing", HasMarketingConsent = true, ValidTxCount = 26, DistinctSectorCount = 5, PortfolioRealSpendQuantile = 0.95m, SpendDecile = 10
            },
            // 1004: Gold Dormant Card
            new()
            {
                CustomerId = 1004, CustomerExternalNo = "CUST-1004", FullName = "Elif Sahin", PrestigeSegment = "Gold",
                TotalCreditLimit = 120000m, AvailableCreditLimit = 120000m, AvailableLimitRatio = 1.0000m,
                MisMonthlyNetProfit = 5400m, HistoricalBaselineSpend = 450m, RealBaselineSpendIndex = 520m,
                SpendElasticityPersona = "Dormant_Sleeper", IsDormant = true, DaysSinceLastTx = 115, HasMarketingConsent = true, PortfolioRealSpendQuantile = 0.10m, SpendDecile = 1
            },
            // 1005: Standard Persuadable
            new()
            {
                CustomerId = 1005, CustomerExternalNo = "CUST-1005", FullName = "Caner Ozturk", PrestigeSegment = "Standard",
                TotalCreditLimit = 60000m, AvailableCreditLimit = 45000m, AvailableLimitRatio = 0.7500m,
                MisMonthlyNetProfit = 980m, HistoricalBaselineSpend = 11000m, RealBaselineSpendIndex = 11800m,
                SpendElasticityPersona = "Persuadable", HasMarketingConsent = true, ValidTxCount = 5, DistinctSectorCount = 2, PortfolioRealSpendQuantile = 0.35m, SpendDecile = 4
            },
            // 1006: Customer with Very Low Available Limit (AvailableCreditLimit = 2,000 TRY -> 80% = 1,600 TRY, so any target above 1,600 TRY must be blocked)
            new()
            {
                CustomerId = 1006, CustomerExternalNo = "CUST-1006", FullName = "Selin Arslan", PrestigeSegment = "Standard",
                TotalCreditLimit = 30000m, AvailableCreditLimit = 2000m, AvailableLimitRatio = 0.0667m,
                MisMonthlyNetProfit = 620m, HistoricalBaselineSpend = 8000m, RealBaselineSpendIndex = 8400m,
                SpendElasticityPersona = "Persuadable", HasMarketingConsent = true, ValidTxCount = 4, DistinctSectorCount = 2, PortfolioRealSpendQuantile = 0.25m, SpendDecile = 3
            }
        };

        var predictionsResult = rLearner.PredictPortfolioCausalUplift(customers, campaign);
        Assert.IsTrue(predictionsResult.IsSuccess);

        // Act
        var milpResult = optimizer.OptimizeCampaignPortfolio(campaign, customers, predictionsResult.Value);

        // Assert
        Assert.IsTrue(milpResult.IsSuccess);
        var summary = milpResult.Value;

        // 1. Budget constraint must not be exceeded
        Assert.IsTrue(summary.TotalAllocatedRewardBudget <= campaign.TotalBudget);
        // 2. KPI-2: Free-Rider must be excluded and ratio <= 25%
        Assert.AreEqual(1, summary.ExcludedFreeRiderCount);
        Assert.IsTrue(summary.FreeRiderRatio <= KpiThresholds.MaxFreeRiderRatio);
        Assert.IsFalse(summary.Assignments.Any(a => a.CustomerId == 1003));
        // 3. Customer 1006 with insufficient available limit (2,000 TRY) must not be assigned a Treatment target exceeding their limit
        Assert.IsFalse(summary.Assignments.Any(a => a.CustomerId == 1006 && a.ExperimentGroup == "Treatment"));
        // 4. KPI-8: Monotonicity / Algorithmic Fairness violations must be 0% (0 count)
        Assert.AreEqual(KpiThresholds.MaxAllowedFairnessRuleViolations, summary.FairnessMonotonicityViolationCount);
        // 5. KPI-6: Solve duration must be < 15 minutes (900 seconds)
        Assert.IsTrue(summary.SolveDurationSeconds < KpiThresholds.MaxMilpSolveDurationSeconds);
        // 6. Net incremental economic profit must be positive
        Assert.IsTrue(summary.TotalExpectedIncrementalNetProfit > 0m);
    }
}
