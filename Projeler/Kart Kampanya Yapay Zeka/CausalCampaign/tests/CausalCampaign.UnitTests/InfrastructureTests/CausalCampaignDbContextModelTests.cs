using CausalCampaign.Domain.Entities;
using CausalCampaign.Infrastructure.Persistence;
using CausalCampaign.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.InfrastructureTests;

[TestClass]
public class CausalCampaignDbContextModelTests
{
    private static CausalCampaignDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CausalCampaignDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CausalCampaignDbContext(options);
    }

    [TestMethod]
    public void DbContextModel_AllEightCoreTables_UseCasualCampaignPrefix()
    {
        // Arrange
        using var context = CreateInMemoryContext(nameof(DbContextModel_AllEightCoreTables_UseCasualCampaignPrefix));
        var model = context.Model;

        // Act & Assert
        Assert.AreEqual("CasualCampaign_CAMPAIGN_TEMPLATE", model.FindEntityType(typeof(CampaignTemplate))?.GetTableName());
        Assert.AreEqual("CasualCampaign_CUSTOMER_PROFILE_MIS", model.FindEntityType(typeof(CustomerProfileMis))?.GetTableName());
        Assert.AreEqual("CasualCampaign_INFLATION_INDEX_MONTHLY", model.FindEntityType(typeof(InflationIndexMonthly))?.GetTableName());
        Assert.AreEqual("CasualCampaign_MERCHANT_MASTER_CACHE", model.FindEntityType(typeof(MerchantMasterCache))?.GetTableName());
        Assert.AreEqual("CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT", model.FindEntityType(typeof(CustomerOfferAssignment))?.GetTableName());
        Assert.AreEqual("CasualCampaign_TRANSACTION_LEDGER_FEED", model.FindEntityType(typeof(TransactionLedgerFeed))?.GetTableName());
        Assert.AreEqual("CasualCampaign_OFFER_PROGRESS_TRACKER", model.FindEntityType(typeof(OfferProgressTracker))?.GetTableName());
        Assert.AreEqual("CasualCampaign_REWARD_FULFILLMENT_QUEUE", model.FindEntityType(typeof(RewardFulfillmentQueue))?.GetTableName());
        Assert.AreEqual("vw_CasualCampaign_CustomerCausalFeatures", model.FindEntityType(typeof(CustomerCausalFeatureView))?.GetViewName());
    }

    [TestMethod]
    public void DbContextModel_FinancialAndStateCriticalEntities_HaveRowVersionConcurrencyTokensConfigured()
    {
        // Arrange
        using var context = CreateInMemoryContext(nameof(DbContextModel_FinancialAndStateCriticalEntities_HaveRowVersionConcurrencyTokensConfigured));

        var trackerRowVersion = context.Model.FindEntityType(typeof(OfferProgressTracker))?.FindProperty(nameof(OfferProgressTracker.RowVersion));
        var campaignRowVersion = context.Model.FindEntityType(typeof(CampaignTemplate))?.FindProperty(nameof(CampaignTemplate.RowVersion));
        var rewardRowVersion = context.Model.FindEntityType(typeof(RewardFulfillmentQueue))?.FindProperty(nameof(RewardFulfillmentQueue.RowVersion));

        // Assert
        Assert.IsNotNull(trackerRowVersion);
        Assert.IsTrue(trackerRowVersion.IsConcurrencyToken);

        Assert.IsNotNull(campaignRowVersion);
        Assert.IsTrue(campaignRowVersion.IsConcurrencyToken);

        Assert.IsNotNull(rewardRowVersion);
        Assert.IsTrue(rewardRowVersion.IsConcurrencyToken);
    }

    [TestMethod]
    public async Task SyntheticPortfolioGenerator_WorksCorrectlyWithEfCore()
    {
        // Arrange
        using var context = CreateInMemoryContext(nameof(SyntheticPortfolioGenerator_WorksCorrectlyWithEfCore));
        context.AddEntity(new MerchantMasterCache
        {
            RawMerchantKey = "IYZICO*TRENDYOL.COM ISTANBUL",
            RawMcc_DE18 = "5399",
            CleanedBrandName = "TRENDYOL",
            ResolvedSector = "E-Commerce & Marketplace",
            SubMerchantProvider = "IYZICO",
            ClassificationLayer = 2,
            ConfidenceScore = 0.98m
        });
        await context.SaveChangesAsync();

        var generator = new EfCoreSyntheticPortfolioGenerator(context);

        // Act
        var cachedMerchant = await context.MerchantMasterCachesSet
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.RawMerchantKey == "IYZICO*TRENDYOL.COM ISTANBUL");

        var seedResult = await generator.EnsureSyntheticPortfolioSizeAsync(25, CancellationToken.None);
        int totalCustomers = await context.CustomerProfilesSet.CountAsync();

        // Assert
        Assert.IsNotNull(cachedMerchant);
        Assert.AreEqual("TRENDYOL", cachedMerchant.CleanedBrandName);
        Assert.IsTrue(seedResult.IsSuccess);
        Assert.AreEqual(25, seedResult.Value);
        Assert.AreEqual(25, totalCustomers);
    }
}
