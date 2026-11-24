using CausalCampaign.Application.Services;
using CausalCampaign.Application.Validators;
using CausalCampaign.CausalAi.Bandit;
using CausalCampaign.CausalAi.Explainability;
using CausalCampaign.CausalAi.Recommendation;
using CausalCampaign.CausalAi.Uplift;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Infrastructure.Persistence;
using CausalCampaign.Optimization.Buckets;
using CausalCampaign.Optimization.Bundles;
using CausalCampaign.Optimization.Experiment;
using CausalCampaign.Optimization.Solvers;
using CausalCampaign.PosEnrichment.Caching;
using CausalCampaign.PosEnrichment.Layers;
using CausalCampaign.PosEnrichment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.ApplicationTests;

[TestClass]
public class ApplicationEdgeCasesTests
{
    private static (CausalCampaignDbContext Db, RealtimeOfferTrackerService TrackerService, CampaignManagementAppService AppService) CreateEnv(
        string dbName,
        string targetSector = "All",
        decimal totalBudget = 10000m)
    {
        var options = new DbContextOptionsBuilder<CausalCampaignDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var db = new CausalCampaignDbContext(options);

        db.AddEntity(new CampaignTemplate
        {
            CampaignId = 1,
            CampaignCode = "CMP-EDGE-01",
            CampaignName = "Edge Case Campaign",
            TargetSector = targetSector,
            DefaultRewardType = "RewardPoints",
            TotalBudget = totalBudget,
            ConsumedBudget = 0m,
            MaxRewardPerCustomer = 2500m,
            StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            IsActive = true
        });

        db.AddEntity(new CustomerProfileMis
        {
            CustomerId = 1001,
            CustomerExternalNo = "CUST-1001",
            FullName = "Test Customer",
            PrestigeSegment = "Gold",
            TotalCreditLimit = 100000m,
            AvailableCreditLimit = 80000m,
            MisMonthlyNetProfit = 2500m,
            HistoricalBaselineSpend = 15000m,
            RealBaselineSpendIndex = 16000m,
            SpendElasticityPersona = "Persuadable",
            HasMarketingConsent = true
        });

        db.AddEntity(new CustomerOfferAssignment
        {
            AssignmentId = 1,
            CampaignId = 1,
            CustomerId = 1001,
            MicroBucketId = 12,
            MicroBucketName = "Bucket-12",
            ExperimentGroup = "Treatment",
            AssignedTargetAmount = 20000m,
            AssignedRewardAmount = 500m,
            RewardType = "RewardPoints",
            AlternativeOffersJson = """
            [
              {"Index":1,"Title":"Spend 20,000 TRY, Earn 500 TRY Points","Target":20000,"Reward":500,"RewardType":"RewardPoints"},
              {"Index":2,"Title":"Spend 20,000 TRY, Earn 460 TRY Cashback","Target":20000,"Reward":460,"RewardType":"Cashback"},
              {"Index":3,"Title":"Spend 11,000 TRY in Fuel, Earn 450 TRY Points","Target":11000,"Reward":450,"RewardType":"RewardPoints"}
            ]
            """
        });

        db.AddEntity(new OfferProgressTracker
        {
            TrackerId = 1,
            AssignmentId = 1,
            CustomerId = 1001,
            CampaignId = 1,
            TargetAmount = 20000m,
            CurrentQualifiedSpend = 15000m,
            RemainingAmount = 5000m,
            ProgressPercentage = 75.00m,
            QualifiedTxCount = 3
        });

        db.SaveChanges();

        var layer2 = new PaymentFacilitatorRegexLayer();
        var posService = new PosEnrichmentService(
            new PosMerchantCacheManager(),
            new DeterministicBrandDictionaryLayer(),
            layer2,
            new LocalMlMerchantClassifierLayer(layer2));

        var trackerService = new RealtimeOfferTrackerService(db, posService);
        var appService = new CampaignManagementAppService(
            db,
            new RobinsonRLearnerUpliftEngine(
                new DormantCardReactivationModel(),
                new MatrixFactorizationSectorRecommender(),
                new ThompsonSamplingBanditService(),
                new XaiTurkishExplanationService()),
            new MilpPortfolioOptimizer(
                new MicroBucketCatalogService(),
                new EquivalentOfferBundleBuilder(),
                new StratifiedRctHoldoutSplitter()));

        return (db, trackerService, appService);
    }

    [TestMethod]
    public async Task ProcessAuthorizationAsync_PreventsDuplicateAuthReplay_AndAllocatesAndReleasesBudgetOnClawback()
    {
        var (db, trackerService, _) = CreateEnv(nameof(ProcessAuthorizationAsync_PreventsDuplicateAuthReplay_AndAllocatesAndReleasesBudgetOnClawback));
        var txTime = new DateTime(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);
        var req = new AuthorizeTransactionRequestDto("AUTH-DUP-1", 1001, 1, 6000m, "TRY", "5411", "MIGROS TICARET", txTime);

        var first = await trackerService.ProcessAuthorizationAsync(req);
        Assert.IsTrue(first.IsSuccess);
        Assert.IsTrue(first.Value.IsGoalCompleted);
        Assert.AreEqual(500m, (await db.CampaignTemplatesSet.FirstAsync()).ConsumedBudget);

        // Replay attack with identical AuthorizationCode -> Conflict!
        var replay = await trackerService.ProcessAuthorizationAsync(req);
        Assert.IsTrue(replay.IsFailure);
        Assert.AreEqual("DUPLICATE_AUTHORIZATION_CODE", replay.Error.Code);

        // Refund drops spend below 20,000 -> Clawback + releases 500m campaign budget!
        var rev = await trackerService.ProcessReversalOrRefundAsync(new ReversalTransactionRequestDto(
            "REV-DUP-1", 1001, 1, first.Value.TransactionId, 3000m, txTime.AddHours(1)));
        Assert.IsTrue(rev.IsSuccess);
        Assert.IsTrue(rev.Value.RewardClawedBack);
        Assert.AreEqual(0m, (await db.CampaignTemplatesSet.FirstAsync()).ConsumedBudget);

        // Replaying the same ReversalAuthCode -> Conflict!
        var revReplay = await trackerService.ProcessReversalOrRefundAsync(new ReversalTransactionRequestDto(
            "REV-DUP-1", 1001, 1, first.Value.TransactionId, 3000m, txTime.AddHours(1)));
        Assert.IsTrue(revReplay.IsFailure);
        Assert.AreEqual("DUPLICATE_REVERSAL_CODE", revReplay.Error.Code);
    }

    [TestMethod]
    public async Task ProcessAuthorizationAsync_BlocksOutOfOrderSplitTx_AndDoesNotMisclassifySectorMismatchAsAntiGaming()
    {
        var (_, trackerService, _) = CreateEnv(
            nameof(ProcessAuthorizationAsync_BlocksOutOfOrderSplitTx_AndDoesNotMisclassifySectorMismatchAsAntiGaming),
            targetSector: "Fuel");
        var t1 = new DateTime(2026, 11, 22, 14, 0, 0, DateTimeKind.Utc);

        // 1. Legitimate non-target sector (Grocery during Fuel campaign) -> NOT accepted, NOT Anti-Gaming fraud
        var groceryRes = await trackerService.ProcessAuthorizationAsync(
            new AuthorizeTransactionRequestDto("AUTH-SEC-1", 1001, 1, 1000m, "TRY", "5411", "MIGROS TICARET", t1));
        Assert.IsTrue(groceryRes.IsSuccess);
        Assert.IsFalse(groceryRes.Value.IsAcceptedForCampaign);
        Assert.IsFalse(groceryRes.Value.IsExcludedByAntiGaming);

        // 2. Qualified Fuel transaction at t1
        var fuel1 = await trackerService.ProcessAuthorizationAsync(
            new AuthorizeTransactionRequestDto("AUTH-FUEL-1", 1001, 1, 1000m, "TRY", "5541", "SHELL PETROL", t1));
        Assert.IsTrue(fuel1.IsSuccess);
        Assert.IsTrue(fuel1.Value.IsAcceptedForCampaign);

        // 3. Out-of-order / backdated transaction at same brand (t1 - 10 min) -> Symmetric window blocks as Split-Transaction!
        var backdatedSplit = await trackerService.ProcessAuthorizationAsync(
            new AuthorizeTransactionRequestDto("AUTH-FUEL-2", 1001, 1, 1000m, "TRY", "5541", "SHELL PETROL", t1.AddMinutes(-10)));
        Assert.IsTrue(backdatedSplit.IsSuccess);
        Assert.IsFalse(backdatedSplit.Value.IsAcceptedForCampaign);
        Assert.IsTrue(backdatedSplit.Value.IsExcludedByAntiGaming);
    }

    [TestMethod]
    public async Task ProcessReversalOrRefundAsync_RejectsOverRefundAndCustomerMismatch()
    {
        var (_, trackerService, _) = CreateEnv(nameof(ProcessReversalOrRefundAsync_RejectsOverRefundAndCustomerMismatch));
        var t1 = new DateTime(2026, 11, 22, 16, 0, 0, DateTimeKind.Utc);

        var tx = await trackerService.ProcessAuthorizationAsync(
            new AuthorizeTransactionRequestDto("AUTH-ORIG-1", 1001, 1, 2000m, "TRY", "5411", "MIGROS TICARET", t1));
        Assert.IsTrue(tx.IsSuccess);

        // Over-refund (3,000 > 2,000 original)
        var overRefund = await trackerService.ProcessReversalOrRefundAsync(
            new ReversalTransactionRequestDto("REV-OVER-1", 1001, 1, tx.Value.TransactionId, 3000m, t1.AddHours(1)));
        Assert.IsTrue(overRefund.IsFailure);
        Assert.AreEqual("REFUND_EXCEEDS_ORIGINAL_AMOUNT", overRefund.Error.Code);

        // Wrong CustomerId trying to reverse Customer 1001's transaction
        var wrongCust = await trackerService.ProcessReversalOrRefundAsync(
            new ReversalTransactionRequestDto("REV-IDOR-1", 9999, 1, tx.Value.TransactionId, 500m, t1.AddHours(1)));
        Assert.IsTrue(wrongCust.IsFailure);
        Assert.AreEqual("ORIGINAL_TRANSACTION_CUSTOMER_MISMATCH", wrongCust.Error.Code);
    }

    [TestMethod]
    public async Task CampaignManagementAppService_SelectOfferQueuesRewardOnInstantCompletion_AndBlocksSwitchAfterCompletion()
    {
        var (db, _, appService) = CreateEnv(nameof(CampaignManagementAppService_SelectOfferQueuesRewardOnInstantCompletion_AndBlocksSwitchAfterCompletion));

        // Current spend is 15,000 TRY; selecting Option 3 (Target = 11,000 TRY) immediately completes the goal and queues reward!
        var selectRes = await appService.SelectCustomerOfferPreferenceAsync(new SelectAlternativeOfferRequestDto(1, 1001, 3));
        Assert.IsTrue(selectRes.IsSuccess);
        Assert.AreEqual(1, await db.RewardFulfillmentQueuesSet.CountAsync());
        Assert.AreEqual(450m, (await db.CampaignTemplatesSet.FirstAsync()).ConsumedBudget);

        // Subsequent offer switch after completion must be rejected
        var secondSwitch = await appService.SelectCustomerOfferPreferenceAsync(new SelectAlternativeOfferRequestDto(1, 1001, 2));
        Assert.IsTrue(secondSwitch.IsFailure);
        Assert.AreEqual("OFFER_ALREADY_COMPLETED", secondSwitch.Error.Code);

        // Audit for another campaign (ID=99) must NOT leak Campaign 1's assignment
        var otherCampaignAudit = await appService.GetCustomerCallCenterAuditAsync(1001, 99);
        Assert.IsTrue(otherCampaignAudit.IsSuccess);
        Assert.IsNull(otherCampaignAudit.Value.ActiveAssignment);
    }

    [TestMethod]
    public void Validators_RejectNonTryCurrencyAndWhitespaceKillSwitchReason()
    {
        var authValidator = new AuthorizeTransactionRequestValidator();
        var usdRes = authValidator.Validate(new AuthorizeTransactionRequestDto(
            "AUTH-FX", 1001, 1, 100m, "USD", "5411", "MIGROS TICARET", DateTime.UtcNow));
        Assert.IsFalse(usdRes.IsValid);

        var killValidator = new TriggerKillSwitchRequestValidator();
        var wsRes = killValidator.Validate(new TriggerKillSwitchRequestDto(1, "  a  ", "Admin"));
        Assert.IsFalse(wsRes.IsValid);

        var revValidator = new ReversalTransactionRequestValidator();
        Assert.IsFalse(revValidator.Validate(new ReversalTransactionRequestDto("", 0, 0, 0, -10m, DateTime.UtcNow)).IsValid);
    }
}
