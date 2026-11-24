using System.Text.Json;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.ValueObjects;
using CausalCampaign.Optimization.Bundles;
using CausalCampaign.Optimization.Experiment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.OptimizationTests;

[TestClass]
public class EquivalentOfferAndStratifiedRctTests
{
    [TestMethod]
    public void BuildEquivalentOffersJson_ProducesThreeDistinctOptionsWithinCreditLimitAndRewardCap()
    {
        // Arrange
        var builder = new EquivalentOfferBundleBuilder();

        // Act
        string json = builder.BuildEquivalentOffersJson(
            primaryTargetAmount: 36000m,
            primaryRewardAmount: 750m,
            primaryRewardType: "RewardPoints",
            recommendedSector: "E-Commerce & Marketplace",
            maxAllowedTargetAmount: 88000m,
            maxRewardPerCustomer: 2500m);

        var options = JsonSerializer.Deserialize<List<AlternativeOfferOption>>(json);

        // Assert
        Assert.IsNotNull(options);
        Assert.AreEqual(3, options.Count);
        Assert.IsTrue(options.All(o => o.Target <= 88000m && o.Reward <= 2500m));
        Assert.IsTrue(options.Any(o => o.RewardType == "Cashback"));
        Assert.IsTrue(options.Any(o => o.RewardType == "RewardPoints"));
    }

    [TestMethod]
    public void SelectStratifiedControlGroupCustomerIds_SelectsTenPercentHoldoutCohort()
    {
        // Arrange
        var splitter = new StratifiedRctHoldoutSplitter();
        var customers = Enumerable.Range(1, 20)
            .Select(i => new CustomerCausalFeatureView
            {
                CustomerId = 1000 + i,
                PrestigeSegment = i % 2 == 0 ? "Gold" : "Silver",
                SpendDecile = (i % 10) + 1
            })
            .ToList();

        // Act
        var holdoutIds = splitter.SelectStratifiedControlGroupCustomerIds(customers, holdoutRatio: 0.10m);

        // Assert
        Assert.AreEqual(2, holdoutIds.Count);
    }
}
