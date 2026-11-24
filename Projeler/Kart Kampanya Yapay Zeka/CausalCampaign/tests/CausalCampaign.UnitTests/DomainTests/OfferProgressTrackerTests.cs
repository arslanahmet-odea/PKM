using CausalCampaign.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.DomainTests;

[TestClass]
public class OfferProgressTrackerTests
{
    [TestMethod]
    public void ApplyQualifiedTransaction_WhenSpendBelowTarget_UpdatesProgressAndDoesNotComplete()
    {
        // Arrange
        var tracker = new OfferProgressTracker
        {
            TrackerId = 1,
            AssignmentId = 10,
            CustomerId = 1001,
            CampaignId = 1,
            TargetAmount = 36000.00m,
            CurrentQualifiedSpend = 15000.00m,
            RemainingAmount = 21000.00m
        };
        var txTime = new DateTime(2026, 11, 22, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = tracker.ApplyQualifiedTransaction(9000.00m, txTime);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.Value);
        Assert.AreEqual(24000.00m, tracker.CurrentQualifiedSpend);
        Assert.AreEqual(12000.00m, tracker.RemainingAmount);
        Assert.AreEqual(66.67m, tracker.ProgressPercentage);
        Assert.IsFalse(tracker.IsCompleted);
        Assert.IsNull(tracker.CompletedAt);
    }

    [TestMethod]
    public void ApplyQualifiedTransaction_WhenSpendReachesTarget_MarksCompletedAndReturnsTrueOnce()
    {
        // Arrange
        var tracker = new OfferProgressTracker
        {
            TrackerId = 1,
            TargetAmount = 20000.00m,
            CurrentQualifiedSpend = 14000.00m,
            RemainingAmount = 6000.00m
        };
        var txTime = new DateTime(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);

        // Act 1: First completion
        var firstCompletion = tracker.ApplyQualifiedTransaction(7500.00m, txTime);
        // Act 2: Subsequent spend after completion
        var secondSpend = tracker.ApplyQualifiedTransaction(1500.00m, txTime.AddMinutes(15));

        // Assert
        Assert.IsTrue(firstCompletion.IsSuccess);
        Assert.IsTrue(firstCompletion.Value);
        Assert.IsTrue(tracker.IsCompleted);
        Assert.AreEqual(100.00m, tracker.ProgressPercentage);
        Assert.AreEqual(0.00m, tracker.RemainingAmount);
        Assert.AreEqual(txTime, tracker.CompletedAt);

        Assert.IsTrue(secondSpend.IsSuccess);
        Assert.IsFalse(secondSpend.Value);
    }

    [TestMethod]
    public void ApplyReversalOrRefund_WhenCompletedTrackerDropsBelowTarget_TriggersClawbackAndMarksReward()
    {
        // Arrange
        var completionTime = new DateTime(2026, 11, 21, 14, 0, 0, DateTimeKind.Utc);
        var tracker = new OfferProgressTracker
        {
            TrackerId = 3,
            TargetAmount = 5000.00m,
            CurrentQualifiedSpend = 5200.00m,
            RemainingAmount = 0.00m,
            ProgressPercentage = 100.00m,
            IsCompleted = true,
            CompletedAt = completionTime
        };
        var rewardQueue = new RewardFulfillmentQueue
        {
            FulfillmentId = 1,
            TrackerId = 3,
            RewardAmount = 400.00m,
            RewardType = "Cashback",
            Status = "PendingSettlement"
        };
        var refundTime = completionTime.AddHours(6);

        // Act
        var refundResult = tracker.ApplyReversalOrRefund(1200.00m, refundTime);
        if (refundResult.IsSuccess && refundResult.Value)
        {
            rewardQueue.TriggerClawback("Clawback triggered: qualified spend dropped below the 5,000 TRY target due to refund.", refundTime);
        }

        // Assert
        Assert.IsTrue(refundResult.IsSuccess);
        Assert.IsTrue(refundResult.Value);
        Assert.IsFalse(tracker.IsCompleted);
        Assert.IsNull(tracker.CompletedAt);
        Assert.AreEqual(4000.00m, tracker.CurrentQualifiedSpend);
        Assert.AreEqual(1000.00m, tracker.RemainingAmount);
        Assert.AreEqual(80.00m, tracker.ProgressPercentage);
        Assert.AreEqual("ClawedBack", rewardQueue.Status);
        Assert.AreEqual(refundTime, rewardQueue.ClawedBackAt);
    }
}
