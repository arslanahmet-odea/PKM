using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.DomainTests;

[TestClass]
public class DomainEdgeCasesAndInvariantsTests
{
    [TestMethod]
    public void Result_AllowsNullableSuccessValue_AndThrowsWhenAccessingFailedValue()
    {
        var okNull = Result<string?>.Success(null);
        Assert.IsTrue(okNull.IsSuccess);
        Assert.IsNull(okNull.Value);

        var failed = Result<int>.Failure(DomainError.Validation("ERR", "Invalid"));
        Assert.IsTrue(failed.IsFailure);
        Assert.ThrowsException<InvalidOperationException>(() => _ = failed.Value);
    }

    [TestMethod]
    public void CampaignTemplate_AllocateAndReleaseBudget_EnforcesKillSwitchInactiveAndBounds()
    {
        var campaign = new CampaignTemplate
        {
            CampaignId = 1,
            CampaignCode = "CMP-EDGE-01",
            TotalBudget = 1000m,
            ConsumedBudget = 0m,
            MaxRewardPerCustomer = 500m,
            StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 11, 30, 23, 59, 59, DateTimeKind.Utc),
            IsActive = true
        };

        // Boundary date checks
        Assert.IsTrue(campaign.VerifyOperationalStatus(campaign.StartDate).IsSuccess);
        Assert.IsTrue(campaign.VerifyOperationalStatus(campaign.EndDate).IsSuccess);
        Assert.IsTrue(campaign.VerifyOperationalStatus(campaign.StartDate.AddTicks(-1)).IsFailure);
        Assert.IsTrue(campaign.VerifyOperationalStatus(campaign.EndDate.AddTicks(1)).IsFailure);

        // Allocate & Release
        Assert.IsTrue(campaign.AllocateRewardBudget(500m).IsSuccess);
        Assert.AreEqual(500m, campaign.ConsumedBudget);
        Assert.IsTrue(campaign.ReleaseRewardBudget(200m).IsSuccess);
        Assert.AreEqual(300m, campaign.ConsumedBudget);
        Assert.IsTrue(campaign.ReleaseRewardBudget(1000m).IsSuccess);
        Assert.AreEqual(0m, campaign.ConsumedBudget); // Never goes negative
        Assert.IsTrue(campaign.ReleaseRewardBudget(0m).IsFailure);

        // Kill-Switch blocks subsequent budget allocation
        Assert.IsTrue(campaign.TriggerKillSwitch("   a   ").IsFailure);
        Assert.IsTrue(campaign.TriggerKillSwitch(new string('X', 301)).IsFailure);
        Assert.IsTrue(campaign.TriggerKillSwitch("Valid emergency stop reason").IsSuccess);

        var allocWhileHalted = campaign.AllocateRewardBudget(100m);
        Assert.IsTrue(allocWhileHalted.IsFailure);
        Assert.AreEqual("CAMPAIGN_KILL_SWITCH_ACTIVE", allocWhileHalted.Error.Code);
    }

    [TestMethod]
    public void OfferProgressTracker_NeverRoundsTo100PercentBeforeCompletion_AndDecrementsTxCountOnRefund()
    {
        var tracker = new OfferProgressTracker
        {
            TrackerId = 10,
            TargetAmount = 1000.00m,
            CurrentQualifiedSpend = 0m,
            RemainingAmount = 1000.00m
        };
        var now = new DateTime(2026, 11, 22, 10, 0, 0, DateTimeKind.Utc);

        // 999.99 / 1000.00 = 99.999% -> must cap at 99.99%, NOT 100.00%
        var res = tracker.ApplyQualifiedTransaction(999.99m, now);
        Assert.IsTrue(res.IsSuccess);
        Assert.IsFalse(res.Value);
        Assert.IsFalse(tracker.IsCompleted);
        Assert.AreEqual(0.01m, tracker.RemainingAmount);
        Assert.AreEqual(99.99m, tracker.ProgressPercentage);
        Assert.AreEqual(1, tracker.QualifiedTxCount);

        // Refund decrements QualifiedTxCount and clamps spend to 0
        var refundRes = tracker.ApplyReversalOrRefund(1500m, now.AddMinutes(5));
        Assert.IsTrue(refundRes.IsSuccess);
        Assert.IsFalse(refundRes.Value); // Was not completed
        Assert.AreEqual(0m, tracker.CurrentQualifiedSpend);
        Assert.AreEqual(0, tracker.QualifiedTxCount);
        Assert.AreEqual(0m, tracker.ProgressPercentage);
    }

    [TestMethod]
    public void RewardFulfillmentQueue_EnforcesLoadAndClawbackIdempotency()
    {
        var now = DateTime.UtcNow;
        var reward = new RewardFulfillmentQueue
        {
            FulfillmentId = 1,
            TrackerId = 1,
            RewardAmount = 500m,
            Status = "PendingSettlement"
        };

        Assert.IsTrue(reward.MarkAsLoaded(now).IsSuccess);
        Assert.AreEqual("Loaded", reward.Status);

        // Second MarkAsLoaded must fail with conflict
        var secondLoad = reward.MarkAsLoaded(now.AddMinutes(1));
        Assert.IsTrue(secondLoad.IsFailure);
        Assert.AreEqual("REWARD_ALREADY_LOADED", secondLoad.Error.Code);

        // Empty clawback reason fails
        Assert.IsTrue(reward.TriggerClawback("   ", now.AddMinutes(2)).IsFailure);

        // Valid clawback succeeds once, second fails
        Assert.IsTrue(reward.TriggerClawback("Full refund dropped spend below target.", now.AddMinutes(3)).IsSuccess);
        var secondClawback = reward.TriggerClawback("Duplicate clawback.", now.AddMinutes(4));
        Assert.IsTrue(secondClawback.IsFailure);
        Assert.AreEqual("REWARD_ALREADY_CLAWED_BACK", secondClawback.Error.Code);

        // Cannot load after clawback
        var loadAfterClawback = reward.MarkAsLoaded(now.AddMinutes(5));
        Assert.IsTrue(loadAfterClawback.IsFailure);
        Assert.AreEqual("CANNOT_LOAD_CLAWED_BACK_REWARD", loadAfterClawback.Error.Code);
    }

    [TestMethod]
    public void CustomerOfferAssignment_BlocksControlHoldoutAndMalformedPayloads()
    {
        var controlAssignment = new CustomerOfferAssignment
        {
            AssignmentId = 1,
            ExperimentGroup = "Control_Holdout",
            AlternativeOffersJson = """[{"Index":1,"Title":"Test","Target":1000,"Reward":100,"RewardType":"Cashback"}]"""
        };

        var controlRes = controlAssignment.SelectAlternativeOffer(1);
        Assert.IsTrue(controlRes.IsFailure);
        Assert.AreEqual("CONTROL_GROUP_OFFER_SELECTION_NOT_ALLOWED", controlRes.Error.Code);

        var malformedAssignment = new CustomerOfferAssignment
        {
            AssignmentId = 2,
            ExperimentGroup = "Treatment",
            AlternativeOffersJson = "{not-valid-json"
        };
        Assert.AreEqual(0, malformedAssignment.GetAlternativeOfferOptions().Count);
        Assert.AreEqual("ALTERNATIVE_OFFER_NOT_FOUND", malformedAssignment.SelectAlternativeOffer(1).Error.Code);

        var badPayloadAssignment = new CustomerOfferAssignment
        {
            AssignmentId = 3,
            ExperimentGroup = "Treatment",
            AlternativeOffersJson = """[{"Index":1,"Title":"Bad","Target":0,"Reward":100,"RewardType":"Cashback"}]"""
        };
        Assert.AreEqual("INVALID_ALTERNATIVE_OFFER_PAYLOAD", badPayloadAssignment.SelectAlternativeOffer(1).Error.Code);
    }

    [TestMethod]
    [DataRow("4829", true)]
    [DataRow("6010", true)]
    [DataRow("6011", true)]
    [DataRow("6012", true)]
    [DataRow("6051", true)]
    [DataRow("6211", true)]
    [DataRow("6540", true)]
    [DataRow("7995", true)]
    [DataRow(" 6051 ", true)]
    [DataRow(null, true)]
    [DataRow("", true)]
    [DataRow("ABCD", true)]
    [DataRow("541", true)]
    [DataRow("54111", true)]
    [DataRow("5411", false)]
    public void AntiGamingConstants_IsExcludedMcc_FailsClosedOnMalformedAndExcludedMccs(string? mcc, bool expectedExcluded)
    {
        Assert.AreEqual(expectedExcluded, AntiGamingConstants.IsExcludedMcc(mcc));
    }
}
