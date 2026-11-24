using CausalCampaign.Application.Services;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.ApplicationTests;

[TestClass]
public class CampaignManagementAppServiceTests
{
    [TestMethod]
    public async Task SelectCustomerOfferPreferenceAsync_And_TriggerKillSwitchAsync_UpdateEntitiesCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CausalCampaignDbContext>()
            .UseInMemoryDatabase(databaseName: nameof(SelectCustomerOfferPreferenceAsync_And_TriggerKillSwitchAsync_UpdateEntitiesCorrectly))
            .Options;

        using var db = new CausalCampaignDbContext(options);

        db.AddEntity(new CampaignTemplate
        {
            CampaignId = 1,
            CampaignCode = "CMP-2026-UPSELL-01",
            CampaignName = "Test Campaign",
            TotalBudget = 1000000m,
            MaxRewardPerCustomer = 2500m,
            IsActive = true
        });

        db.AddEntity(new CustomerProfileMis
        {
            CustomerId = 1001,
            CustomerExternalNo = "CUST-1001",
            FullName = "Ahmet Yilmaz",
            PrestigeSegment = "Gold",
            TotalCreditLimit = 150000m,
            AvailableCreditLimit = 110000m
        });

        db.AddEntity(new CustomerOfferAssignment
        {
            AssignmentId = 1,
            CampaignId = 1,
            CustomerId = 1001,
            AssignedTargetAmount = 36000m,
            AssignedRewardAmount = 750m,
            RewardType = "RewardPoints",
            AlternativeOffersJson = """
            [
              {"Index":1,"Title":"Spend 36,000 TRY, Earn 750 TRY Points","Target":36000,"Reward":750,"RewardType":"RewardPoints"},
              {"Index":2,"Title":"Spend 38,500 TRY, Earn 700 TRY Cashback","Target":38500,"Reward":700,"RewardType":"Cashback"},
              {"Index":3,"Title":"Spend 15,000 TRY in E-Commerce, Earn 800 TRY Points","Target":15000,"Reward":800,"RewardType":"RewardPoints"}
            ]
            """
        });

        db.AddEntity(new OfferProgressTracker
        {
            TrackerId = 1,
            AssignmentId = 1,
            CustomerId = 1001,
            CampaignId = 1,
            TargetAmount = 36000m,
            CurrentQualifiedSpend = 18000m,
            RemainingAmount = 18000m,
            ProgressPercentage = 50.00m
        });

        await db.SaveChangesAsync();

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

        // Act 1: Customer selects option 3 (Sector Advantage with 15,000 TRY Target)
        var prefResult = await appService.SelectCustomerOfferPreferenceAsync(
            new SelectAlternativeOfferRequestDto(AssignmentId: 1, CustomerId: 1001, SelectedOfferIndex: 3));

        // Act 2: Retrieve Call Center audit record
        var auditResult = await appService.GetCustomerCallCenterAuditAsync(1001, 1);

        // Act 3: Trigger Kill-Switch
        var killResult = await appService.TriggerCampaignKillSwitchAsync(
            new TriggerKillSwitchRequestDto(1, "Emergency halt due to budget variance.", "RiskManagement"));

        // Assert
        Assert.IsTrue(prefResult.IsSuccess);
        Assert.AreEqual(15000m, prefResult.Value.Target);

        Assert.IsTrue(auditResult.IsSuccess);
        Assert.IsNotNull(auditResult.Value.ActiveTracker);
        Assert.AreEqual(15000m, auditResult.Value.ActiveTracker.TargetAmount);
        Assert.IsTrue(auditResult.Value.ActiveTracker.IsCompleted); // 18,000 TRY current spend >= 15,000 TRY new target!

        Assert.IsTrue(killResult.IsSuccess);
        var updatedCampaign = await db.CampaignTemplatesSet.FirstAsync();
        Assert.IsTrue(updatedCampaign.KillSwitchTriggered);
    }
}
