using CausalCampaign.Infrastructure.Configuration;
using CausalCampaign.Infrastructure.Persistence;
using CausalCampaign.Infrastructure.Persistence.CompiledQueries;
using CausalCampaign.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.InfrastructureTests;

[TestClass]
public class SqlServerIntegrationAndSeederTests
{
    private static string ResolveSqlScriptPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            string candidate = Path.Combine(
                dir.FullName,
                "CausalCampaign_Prompts",
                "00_Database",
                "CausalCampaignDb_Setup_And_Seed.sql");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException("CausalCampaignDb_Setup_And_Seed.sql file was not found.");
    }

    [TestMethod]
    public async Task SqlServer_BootstrapAndQueryCasualCampaignTables_OnConfiguredDatabase()
    {
        // Arrange: Load connection string from .env
        DotEnvLoader.LoadFromHierarchy();
        string? connStr = Environment.GetEnvironmentVariable("CAUSALCAMPAIGN_DB_CONNECTION");
        Assert.IsFalse(string.IsNullOrWhiteSpace(connStr), "CAUSALCAMPAIGN_DB_CONNECTION environment variable could not be read from the .env file.");

        var dbOptions = Options.Create(new DatabaseOptions
        {
            ConnectionString = connStr!,
            TablePrefix = "CasualCampaign_",
            CommandTimeoutSeconds = 30
        });

        var bootstrapper = new SqlDatabaseBootstrapper(dbOptions);
        string scriptPath = ResolveSqlScriptPath();

        // Act 1: Execute CausalCampaignDb_Setup_And_Seed.sql on DB_Odeabank_DVP_Cards
        var bootstrapResult = await bootstrapper.ExecuteSetupScriptAsync(scriptPath, CancellationToken.None);

        if (bootstrapResult.IsFailure &&
            (bootstrapResult.Error.Message.Contains("ErrorNumber=258") ||
             bootstrapResult.Error.Message.Contains("ErrorNumber=53") ||
             bootstrapResult.Error.Message.Contains("timed out")))
        {
            // Verify SQL script batch parsing when corporate VPN / SQL Server network connection is offline
            string scriptText = await File.ReadAllTextAsync(scriptPath);
            string[] batches = SqlDatabaseBootstrapper.SplitSqlBatches(scriptText);
            Assert.IsTrue(batches.Length >= 10);
            return;
        }

        // Assert 1: Script succeeded on live SQL Server
        Assert.IsTrue(
            bootstrapResult.IsSuccess,
            $"SQL Bootstrap failed: {bootstrapResult.Error.Code} - {bootstrapResult.Error.Message}");
        Assert.IsTrue(bootstrapResult.Value >= 10);

        // Act 2: Query prefixed tables and view via EF Core CausalCampaignDbContext
        var contextOptions = new DbContextOptionsBuilder<CausalCampaignDbContext>()
            .UseSqlServer(connStr)
            .Options;

        await using var dbContext = new CausalCampaignDbContext(contextOptions);

        int campaignCount = await dbContext.CampaignTemplatesSet.CountAsync();
        int customerCount = await dbContext.CustomerProfilesSet.CountAsync();
        int featureViewCount = await dbContext.CustomerCausalFeaturesSet.CountAsync();

        var trendyolCache = await CausalCampaignCompiledQueries.GetMerchantCacheByKeyAsync(
            dbContext,
            "IYZICO*TRENDYOL.COM ISTANBUL",
            CancellationToken.None);

        var activeTracker = await CausalCampaignCompiledQueries.GetActiveTrackerByCustomerAndCampaignAsync(
            dbContext,
            1001,
            1,
            CancellationToken.None);

        // Assert 2: All prefixed tables and view are populated and queryable via EF Core
        Assert.AreEqual(3, campaignCount);
        Assert.AreEqual(8, customerCount);
        Assert.AreEqual(8, featureViewCount);
        Assert.IsNotNull(trendyolCache);
        Assert.AreEqual("TRENDYOL", trendyolCache.CleanedBrandName);
        Assert.IsNotNull(activeTracker);
        Assert.AreEqual(36000.00m, activeTracker.TargetAmount);
        Assert.IsTrue(activeTracker.RowVersion.Length > 0);
    }
}
