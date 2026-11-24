using CausalCampaign.CausalAi.Bandit;
using CausalCampaign.CausalAi.Explainability;
using CausalCampaign.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.CausalAiTests;

[TestClass]
public class XaiAndBanditPreferenceTests
{
    [TestMethod]
    public void XaiTurkishExplanationService_ProducesNonEmptyExplanationAndThreeNormalizedContributions()
    {
        // Arrange
        var xaiService = new XaiTurkishExplanationService();
        var customer = new CustomerCausalFeatureView
        {
            CustomerId = 1001,
            FullName = "Ahmet Yilmaz",
            PrestigeSegment = "Gold",
            AvailableCreditLimit = 110000m,
            RealBaselineSpendIndex = 25800m,
            PortfolioRealSpendQuantile = 0.72m,
            DistinctSectorCount = 4
        };

        // Act
        var (explanationText, topContributions) = xaiService.GenerateCustomerExplanation(
            customer,
            assignedTarget: 36000m,
            assignedReward: 750m,
            rewardType: "RewardPoints");

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(explanationText));
        Assert.IsTrue(explanationText.Contains("36.000") || explanationText.Contains("36,000"));
        Assert.AreEqual(3, topContributions.Count);
        Assert.AreEqual(1.00m, topContributions.Sum(c => c.Impact));
    }

    [TestMethod]
    public void ThompsonSamplingBanditService_RecordCustomerChoiceFeedback_UpdatesBetaDistributionParameters()
    {
        // Arrange
        var bandit = new ThompsonSamplingBanditService();

        // Act
        var updatedPosterior = bandit.RecordCustomerChoiceFeedback("Cashback", acceptedOrCompleted: true);
        string selectedReward = bandit.SelectRewardTypeWithBandit(1002, "Cashback");

        // Assert
        Assert.AreEqual(13.0, updatedPosterior.Alpha);
        Assert.AreEqual(6.0, updatedPosterior.Beta);
        Assert.IsTrue(selectedReward is "Cashback" or "RewardPoints");
    }
}
