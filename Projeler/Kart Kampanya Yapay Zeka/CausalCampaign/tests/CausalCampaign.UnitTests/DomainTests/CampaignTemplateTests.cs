using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.DomainTests;

[TestClass]
public class CampaignTemplateTests
{
    [TestMethod]
    public void TriggerKillSwitch_WithValidReason_DeactivatesCampaignAndBlocksOperationalStatus()
    {
        // Arrange
        var campaign = new CampaignTemplate
        {
            CampaignId = 1,
            CampaignCode = "CMP-2026-UPSELL-01",
            TotalBudget = 2500000.00m,
            StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            IsActive = true
        };

        // Act
        var triggerResult = campaign.TriggerKillSwitch("Campaign halted due to inflationary drift threshold breach.");
        var operationalResult = campaign.VerifyOperationalStatus(new DateTime(2026, 11, 22, 10, 0, 0, DateTimeKind.Utc));

        // Assert
        Assert.IsTrue(triggerResult.IsSuccess);
        Assert.IsTrue(campaign.KillSwitchTriggered);
        Assert.IsFalse(campaign.IsActive);
        Assert.IsTrue(operationalResult.IsFailure);
        Assert.AreEqual(ErrorType.KillSwitchActive, operationalResult.Error.Type);
    }

    [TestMethod]
    public void AllocateRewardBudget_WhenExceedsCustomerCapOrTotalBudget_ReturnsFailure()
    {
        // Arrange
        var campaign = new CampaignTemplate
        {
            CampaignId = 1,
            CampaignCode = "CMP-2026-UPSELL-01",
            TotalBudget = 1000.00m,
            ConsumedBudget = 700.00m,
            MaxRewardPerCustomer = 500.00m
        };

        // Act
        var capExceededResult = campaign.AllocateRewardBudget(600.00m);
        var budgetExhaustedResult = campaign.AllocateRewardBudget(400.00m);
        var validAllocationResult = campaign.AllocateRewardBudget(250.00m);

        // Assert
        Assert.IsTrue(capExceededResult.IsFailure);
        Assert.AreEqual("REWARD_EXCEEDS_CUSTOMER_CAP", capExceededResult.Error.Code);

        Assert.IsTrue(budgetExhaustedResult.IsFailure);
        Assert.AreEqual("CAMPAIGN_BUDGET_EXHAUSTED", budgetExhaustedResult.Error.Code);

        Assert.IsTrue(validAllocationResult.IsSuccess);
        Assert.AreEqual(950.00m, campaign.ConsumedBudget);
        Assert.AreEqual(50.00m, campaign.RemainingBudget);
    }
}
