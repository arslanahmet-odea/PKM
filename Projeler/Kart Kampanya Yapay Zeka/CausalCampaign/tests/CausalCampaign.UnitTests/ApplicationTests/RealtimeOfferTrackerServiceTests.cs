using CausalCampaign.Application.Services;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Infrastructure.Persistence;
using CausalCampaign.PosEnrichment.Caching;
using CausalCampaign.PosEnrichment.Layers;
using CausalCampaign.PosEnrichment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.ApplicationTests;

[TestClass]
public class RealtimeOfferTrackerServiceTests
{
    private static (CausalCampaignDbContext Db, RealtimeOfferTrackerService Service) CreateInMemoryEnvironment(string dbName)
    {
        var options = new DbContextOptionsBuilder<CausalCampaignDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var db = new CausalCampaignDbContext(options);

        db.AddEntity(new CampaignTemplate
        {
            CampaignId = 1,
            CampaignCode = "CMP-2026-UPSELL-01",
            CampaignName = "Total Spend Upsell Campaign",
            TargetSector = "All",
            DefaultRewardType = "RewardPoints",
            TotalBudget = 2500000m,
            MaxRewardPerCustomer = 2500m,
            StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            IsActive = true
        });

        db.AddEntity(new CustomerOfferAssignment
        {
            AssignmentId = 1,
            CampaignId = 1,
            CustomerId = 1001,
            MicroBucketId = 14,
            MicroBucketName = "Bucket-14",
            ExperimentGroup = "Treatment",
            AssignedTargetAmount = 36000m,
            AssignedRewardAmount = 750m,
            RewardType = "RewardPoints"
        });

        db.AddEntity(new OfferProgressTracker
        {
            TrackerId = 1,
            AssignmentId = 1,
            CustomerId = 1001,
            CampaignId = 1,
            TargetAmount = 36000m,
            CurrentQualifiedSpend = 30000m,
            RemainingAmount = 6000m,
            ProgressPercentage = 83.33m,
            QualifiedTxCount = 4
        });

        db.SaveChanges();

        var layer2 = new PaymentFacilitatorRegexLayer();
        var posService = new PosEnrichmentService(
            new PosMerchantCacheManager(),
            new DeterministicBrandDictionaryLayer(),
            layer2,
            new LocalMlMerchantClassifierLayer(layer2));

        var trackerService = new RealtimeOfferTrackerService(db, posService);
        return (db, trackerService);
    }

    [TestMethod]
    public async Task ProcessAuthorizationAsync_ValidTransactionCompletesGoalQueuesRewardAndSub500MsLatency()
    {
        // Arrange
        var (db, service) = CreateInMemoryEnvironment(nameof(ProcessAuthorizationAsync_ValidTransactionCompletesGoalQueuesRewardAndSub500MsLatency));
        var txTime = new DateTime(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);
        var request = new AuthorizeTransactionRequestDto(
            AuthorizationCode: "AUTH-1001",
            CustomerId: 1001,
            CampaignId: 1,
            Amount: 7500m,
            Currency: "TRY",
            RawMcc_DE18: "5399",
            RawMerchantName_DE43: "IYZICO*TRENDYOL.COM ISTANBUL",
            TransactionTimestampUtc: txTime);

        // Act
        var result = await service.ProcessAuthorizationAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Value.IsAcceptedForCampaign);
        Assert.IsTrue(result.Value.IsGoalCompleted);
        Assert.IsTrue(result.Value.RewardQueuedOrLoaded);
        Assert.AreEqual("TRENDYOL", result.Value.ResolvedBrand);
        Assert.AreEqual(37500m, result.Value.CurrentQualifiedSpend);
        Assert.IsTrue(result.Value.ProcessingLatencyMs < KpiThresholds.MaxRealtimeTrackerLatencyMs);
        Assert.AreEqual(1, await db.RewardFulfillmentQueuesSet.CountAsync());
    }

    [TestMethod]
    public async Task ProcessAuthorizationAsync_WhenSecondTransactionAtSameMerchantWithin30Minutes_BlocksViaSplitTransactionAntiGaming()
    {
        // Arrange
        var (_, service) = CreateInMemoryEnvironment(nameof(ProcessAuthorizationAsync_WhenSecondTransactionAtSameMerchantWithin30Minutes_BlocksViaSplitTransactionAntiGaming));
        var baseTime = new DateTime(2026, 11, 22, 14, 0, 0, DateTimeKind.Utc);

        var firstTx = new AuthorizeTransactionRequestDto(
            "AUTH-2001", 1001, 1, 2000m, "TRY", "7399", "PARAM*GETIR PERAKENDE LOJISTIK", baseTime);

        var splitSecondTx = new AuthorizeTransactionRequestDto(
            "AUTH-2002", 1001, 1, 2500m, "TRY", "7399", "PARAM*GETIR PERAKENDE LOJISTIK", baseTime.AddMinutes(12));

        // Act
        var firstResult = await service.ProcessAuthorizationAsync(firstTx);
        var secondResult = await service.ProcessAuthorizationAsync(splitSecondTx);

        // Assert
        Assert.IsTrue(firstResult.IsSuccess);
        Assert.IsTrue(firstResult.Value.IsAcceptedForCampaign);
        Assert.AreEqual(32000m, firstResult.Value.CurrentQualifiedSpend);

        Assert.IsTrue(secondResult.IsSuccess);
        Assert.IsFalse(secondResult.Value.IsAcceptedForCampaign);
        Assert.IsTrue(secondResult.Value.IsExcludedByAntiGaming);
        Assert.AreEqual(32000m, secondResult.Value.CurrentQualifiedSpend); // Spend counter must not increase!
    }

    [TestMethod]
    public async Task ProcessReversalOrRefundAsync_WhenCompletedGoalDropsBelowTarget_ExecutesRewardClawback()
    {
        // Arrange
        var (db, service) = CreateInMemoryEnvironment(nameof(ProcessReversalOrRefundAsync_WhenCompletedGoalDropsBelowTarget_ExecutesRewardClawback));
        var txTime = new DateTime(2026, 11, 22, 15, 0, 0, DateTimeKind.Utc);

        await service.ProcessAuthorizationAsync(new AuthorizeTransactionRequestDto(
            "AUTH-3001", 1001, 1, 7000m, "TRY", "5411", "MIGROS TICARET A.S.", txTime));

        // Act: Apply a 4,000 TRY refund (37,000 TRY -> 33,000 TRY, dropping below the 36,000 TRY target!)
        var reversalResult = await service.ProcessReversalOrRefundAsync(new ReversalTransactionRequestDto(
            ReversalAuthCode: "REV-3001",
            CustomerId: 1001,
            CampaignId: 1,
            OriginalTransactionId: 1,
            RefundAmount: 4000m,
            ReversalTimestampUtc: txTime.AddHours(2)));

        var rewardEntry = await db.RewardFulfillmentQueuesSet.FirstAsync();

        // Assert
        Assert.IsTrue(reversalResult.IsSuccess);
        Assert.IsFalse(reversalResult.Value.IsGoalCompleted);
        Assert.IsTrue(reversalResult.Value.RewardClawedBack);
        Assert.AreEqual(33000m, reversalResult.Value.CurrentQualifiedSpend);
        Assert.AreEqual("ClawedBack", rewardEntry.Status);
    }
}
