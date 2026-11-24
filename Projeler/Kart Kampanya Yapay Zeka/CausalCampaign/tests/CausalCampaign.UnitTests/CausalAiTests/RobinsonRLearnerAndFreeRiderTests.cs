using CausalCampaign.CausalAi.Bandit;
using CausalCampaign.CausalAi.Explainability;
using CausalCampaign.CausalAi.Recommendation;
using CausalCampaign.CausalAi.Uplift;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.CausalAiTests;

[TestClass]
public class RobinsonRLearnerAndFreeRiderTests
{
    private static RobinsonRLearnerUpliftEngine CreateEngine()
    {
        return new RobinsonRLearnerUpliftEngine(
            new DormantCardReactivationModel(),
            new MatrixFactorizationSectorRecommender(),
            new ThompsonSamplingBanditService(),
            new XaiTurkishExplanationService());
    }

    [TestMethod]
    public void PredictPortfolioCausalUplift_SeparatesFreeRiderFromPersuadableAndDormantCards()
    {
        // Arrange
        var engine = CreateEngine();
        var campaign = new CampaignTemplate
        {
            CampaignId = 1,
            CampaignCode = "CMP-2026-UPSELL-01",
            MaxRewardPerCustomer = 2500.00m,
            MinMisProfitThreshold = -250.00m,
            InterchangeMarginRate = 0.0260m
        };

        var portfolio = new List<CustomerCausalFeatureView>
        {
            // 1. Persuadable Customer (Ahmet Yilmaz)
            new()
            {
                CustomerId = 1001,
                CustomerExternalNo = "CUST-1001",
                FullName = "Ahmet Yilmaz",
                PrestigeSegment = "Gold",
                TotalCreditLimit = 150000m,
                AvailableCreditLimit = 110000m,
                AvailableLimitRatio = 0.7333m,
                MisMonthlyNetProfit = 4250m,
                HistoricalBaselineSpend = 24000m,
                RealBaselineSpendIndex = 25800m,
                SpendElasticityPersona = "Persuadable",
                IsDormant = false,
                DaysSinceLastTx = 2,
                HasMarketingConsent = true,
                PreferredRewardType = "RewardPoints",
                BehavioralClusterId = 1,
                ValidTxCount = 8,
                DistinctSectorCount = 4,
                PortfolioRealSpendQuantile = 0.65m
            },
            // 2. Free-Rider Customer (Mehmet Demir - Spends 68,200 TRY/month organically)
            new()
            {
                CustomerId = 1003,
                CustomerExternalNo = "CUST-1003",
                FullName = "Mehmet Demir",
                PrestigeSegment = "Private",
                TotalCreditLimit = 300000m,
                AvailableCreditLimit = 210000m,
                AvailableLimitRatio = 0.7000m,
                MisMonthlyNetProfit = 8900m,
                HistoricalBaselineSpend = 65000m,
                RealBaselineSpendIndex = 68200m,
                SpendElasticityPersona = "FreeRider_SureThing",
                IsDormant = false,
                DaysSinceLastTx = 1,
                HasMarketingConsent = true,
                PreferredRewardType = "RewardPoints",
                BehavioralClusterId = 3,
                ValidTxCount = 26,
                DistinctSectorCount = 5,
                PortfolioRealSpendQuantile = 0.95m
            },
            // 3. Dormant Card (Elif Sahin - No transactions for 115 days)
            new()
            {
                CustomerId = 1004,
                CustomerExternalNo = "CUST-1004",
                FullName = "Elif Sahin",
                PrestigeSegment = "Gold",
                TotalCreditLimit = 120000m,
                AvailableCreditLimit = 120000m,
                AvailableLimitRatio = 1.0000m,
                MisMonthlyNetProfit = 5400m,
                HistoricalBaselineSpend = 450m,
                RealBaselineSpendIndex = 520m,
                SpendElasticityPersona = "Dormant_Sleeper",
                IsDormant = true,
                DaysSinceLastTx = 115,
                HasMarketingConsent = true,
                PreferredRewardType = "Cashback",
                BehavioralClusterId = 4,
                ValidTxCount = 0,
                DistinctSectorCount = 0,
                PortfolioRealSpendQuantile = 0.12m
            }
        };

        // Act
        var result = engine.PredictPortfolioCausalUplift(portfolio, campaign);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(3, result.Value.Count);

        var persuadable = result.Value.First(x => x.CustomerId == 1001);
        var freeRider = result.Value.First(x => x.CustomerId == 1003);
        var dormant = result.Value.First(x => x.CustomerId == 1004);

        Assert.AreEqual("Persuadable", persuadable.PredictedPersona);
        Assert.IsTrue(persuadable.PredictedIncrementalUplift >= 10000m);

        // KPI-2: Free-Rider organic spender must be accurately detected and must not produce fake high uplift
        Assert.AreEqual("FreeRider_SureThing", freeRider.PredictedPersona);
        Assert.IsTrue(freeRider.PredictedIncrementalUplift < freeRider.ExpectedOrganicSpend * 0.05m);

        // KPI-4: Dormant card reactivation probability must be >= 6.5% (0.065)
        Assert.AreEqual("Dormant_Sleeper", dormant.PredictedPersona);
        Assert.IsTrue(
            dormant.ReactivationProbability >= KpiThresholds.MinDormantReactivationRate,
            $"KPI-4 Dormant card reactivation probability ({dormant.ReactivationProbability:P2}) is below target ({KpiThresholds.MinDormantReactivationRate:P2}).");
    }
}
