using System.Diagnostics;
using System.Text.Json;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.ValueObjects;
using CausalCampaign.Optimization.Buckets;
using CausalCampaign.Optimization.Bundles;
using CausalCampaign.Optimization.Experiment;
using CausalCampaign.Optimization.Solvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.OptimizationTests;

[TestClass]
public class OptimizationEdgeCasesTests
{
    [TestMethod]
    public void EquivalentOfferBundleBuilder_AlwaysRespectsBudgetCapCreditLimitAndMonotonicity()
    {
        var builder = new EquivalentOfferBundleBuilder();

        // Standard case
        string json = builder.BuildEquivalentOffersJson(
            primaryTargetAmount: 36000m,
            primaryRewardAmount: 750m,
            primaryRewardType: "RewardPoints",
            recommendedSector: "Fuel",
            maxAllowedTargetAmount: 40000m,
            maxRewardPerCustomer: 1000m);

        var options = JsonSerializer.Deserialize<List<AlternativeOfferOption>>(json)!;
        Assert.AreEqual(3, options.Count);
        Assert.IsTrue(options.All(o => o.Target <= 40000m), "All options must respect maxAllowedTargetAmount.");
        Assert.IsTrue(options.All(o => o.Reward <= 750m), "No alternative option may exceed MILP-budgeted primaryRewardAmount.");
        Assert.AreEqual(0, MicroBucketCatalogService.CountMonotonicityViolations(options.Select(o => (o.Target, o.Reward)).ToList()));

        // Low available credit limit (< 1500 TRY) must still clamp Option 3 target
        string tightJson = builder.BuildEquivalentOffersJson(1200m, 100m, "Cashback", "Grocery", 1000m, 200m);
        var tightOptions = JsonSerializer.Deserialize<List<AlternativeOfferOption>>(tightJson)!;
        Assert.IsTrue(tightOptions.All(o => o.Target <= 1000m));
        Assert.IsTrue(tightOptions.All(o => o.Reward <= 100m));

        // Non-positive target returns empty JSON array
        Assert.AreEqual("[]", builder.BuildEquivalentOffersJson(0m, 100m, "Cashback", "Fuel", 5000m, 200m));
    }

    [TestMethod]
    public void StratifiedRctHoldoutSplitter_HandlesSingleCustomerInvalidRatiosAndTailStrata()
    {
        var splitter = new StratifiedRctHoldoutSplitter();

        // Single customer must NOT be placed in Control_Holdout
        var single = new List<CustomerCausalFeatureView> { new() { CustomerId = 1, PrestigeSegment = "Gold", SpendDecile = 5 } };
        Assert.AreEqual(0, splitter.SelectStratifiedControlGroupCustomerIds(single, 0.10m).Count);

        // Invalid ratios
        var many = Enumerable.Range(1, 30)
            .Select(i => new CustomerCausalFeatureView
            {
                CustomerId = i,
                PrestigeSegment = i <= 10 ? "Gold" : i <= 20 ? "Private" : "Silver",
                SpendDecile = (i % 10) + 1
            })
            .ToList();

        Assert.AreEqual(0, splitter.SelectStratifiedControlGroupCustomerIds(many, 0m).Count);
        Assert.AreEqual(0, splitter.SelectStratifiedControlGroupCustomerIds(many, 1.0m).Count);

        // 10% of 30 = 3 holdout customers across all 3 segments (including tail segment "Silver")
        var holdout = splitter.SelectStratifiedControlGroupCustomerIds(many, 0.10m);
        Assert.AreEqual(3, holdout.Count);
        Assert.IsTrue(holdout.Any(id => id > 20), "Tail stratum ('Silver') must be represented in stratified holdout.");
    }

    [TestMethod]
    public void MicroBucketCatalogService_CountMonotonicityViolations_DetectsViolationsAndScalesLinearithmically()
    {
        Assert.AreEqual(0, MicroBucketCatalogService.CountMonotonicityViolations(null));
        Assert.AreEqual(0, MicroBucketCatalogService.CountMonotonicityViolations([]));

        var withViolation = new List<(decimal, decimal)>
        {
            (10000m, 500m),
            (20000m, 400m) // Higher target, lower reward -> 1 violation!
        };
        Assert.AreEqual(1, MicroBucketCatalogService.CountMonotonicityViolations(withViolation));

        // Performance test on 10,000 assignments (must finish well under 500 ms with O(N log N))
        var largePortfolio = Enumerable.Range(1, 10_000)
            .Select(i => ((decimal)((i % 20 + 1) * 5000), (decimal)((i % 20 + 1) * 100)))
            .ToList();

        var sw = Stopwatch.StartNew();
        int violations = MicroBucketCatalogService.CountMonotonicityViolations(largePortfolio);
        sw.Stop();

        Assert.AreEqual(0, violations);
        Assert.IsTrue(sw.ElapsedMilliseconds < 500, $"Monotonicity check took too long: {sw.ElapsedMilliseconds} ms");
    }

    [TestMethod]
    public void MilpPortfolioOptimizer_RejectsDuplicateCustomerIds_AndShortCircuitsZeroBudget()
    {
        var optimizer = new MilpPortfolioOptimizer(
            new MicroBucketCatalogService(),
            new EquivalentOfferBundleBuilder(),
            new StratifiedRctHoldoutSplitter());

        var campaign = new CampaignTemplate
        {
            CampaignId = 1,
            TotalBudget = 1000m,
            ConsumedBudget = 1000m, // Zero remaining budget
            MaxRewardPerCustomer = 500m,
            MinMisProfitThreshold = -250m,
            InterchangeMarginRate = 0.025m
        };

        var cust1 = new CustomerCausalFeatureView
        {
            CustomerId = 1, CustomerExternalNo = "C1", AvailableCreditLimit = 100000m,
            MisMonthlyNetProfit = 2000m, HasMarketingConsent = true, SpendElasticityPersona = "Persuadable"
        };
        var pred1 = new CustomerCausalPredictionDto(1, "C1", 10000m, 8000m, 0.5m, 0m, "Persuadable", "Fuel", "Cashback", "XAI", []);

        // Duplicate customer ID rejected
        var dupRes = optimizer.OptimizeCampaignPortfolio(campaign, [cust1, cust1], [pred1]);
        Assert.IsTrue(dupRes.IsFailure);
        Assert.AreEqual("DUPLICATE_CUSTOMER_ID_IN_PORTFOLIO", dupRes.Error.Code);

        // Zero remaining budget short-circuits cleanly
        var zeroBudgetRes = optimizer.OptimizeCampaignPortfolio(campaign, [cust1], [pred1]);
        Assert.IsTrue(zeroBudgetRes.IsSuccess);
        Assert.AreEqual("ZERO_BUDGET_SHORT_CIRCUIT", zeroBudgetRes.Value.SolverStatus);
        Assert.AreEqual(0, zeroBudgetRes.Value.AssignedTreatmentCount);
    }
}
