using CausalCampaign.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.DomainTests;

[TestClass]
public class CustomerOfferAssignmentTests
{
    [TestMethod]
    public void SelectAlternativeOffer_WhenCustomerChoosesCashbackOption_UpdatesTargetAndRewardType()
    {
        // Arrange
        var assignment = new CustomerOfferAssignment
        {
            AssignmentId = 1,
            CustomerId = 1001,
            CampaignId = 1,
            AssignedTargetAmount = 36000.00m,
            AssignedRewardAmount = 750.00m,
            RewardType = "RewardPoints",
            CustomerSelectedOfferIndex = 1,
            AlternativeOffersJson = """
            [
              {"Index":1,"Title":"Spend 36,000 TRY, Earn 750 TRY Points","Target":36000,"Reward":750,"RewardType":"RewardPoints"},
              {"Index":2,"Title":"Spend 38,500 TRY, Earn 700 TRY Cashback","Target":38500,"Reward":700,"RewardType":"Cashback"},
              {"Index":3,"Title":"Spend 15,000 TRY in E-Commerce, Earn 800 TRY Points","Target":15000,"Reward":800,"RewardType":"RewardPoints"}
            ]
            """
        };

        // Act
        var selectResult = assignment.SelectAlternativeOffer(2);

        // Assert
        Assert.IsTrue(selectResult.IsSuccess);
        Assert.AreEqual((byte)2, assignment.CustomerSelectedOfferIndex);
        Assert.AreEqual(38500.00m, assignment.AssignedTargetAmount);
        Assert.AreEqual(700.00m, assignment.AssignedRewardAmount);
        Assert.AreEqual("Cashback", assignment.RewardType);
    }

    [TestMethod]
    public void SelectAlternativeOffer_WithOutOfRangeIndex_ReturnsValidationFailure()
    {
        // Arrange
        var assignment = new CustomerOfferAssignment
        {
            AssignmentId = 1,
            AlternativeOffersJson = "[]"
        };

        // Act
        var result = assignment.SelectAlternativeOffer(5);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual("INVALID_OFFER_OPTION_INDEX", result.Error.Code);
    }
}
