using CausalCampaign.Domain.Entities;
using CausalCampaign.Infrastructure.Configuration;
using CausalCampaign.Infrastructure.Persistence;
using CausalCampaign.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.InfrastructureTests;

[TestClass]
public class InfrastructureEdgeCasesTests
{
    [TestMethod]
    public void DotEnvLoader_ParsesQuotesCommentsExportAndPreservesExistingEnvVars()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"causal_test_{Guid.NewGuid():N}.env");
        string key1 = $"CAUSAL_TEST_A_{Guid.NewGuid():N}";
        string key2 = $"CAUSAL_TEST_B_{Guid.NewGuid():N}";
        string key3 = $"CAUSAL_TEST_C_{Guid.NewGuid():N}";

        try
        {
            Environment.SetEnvironmentVariable(key3, "PRE_EXISTING_VALUE");
            File.WriteAllText(tempFile, $"""
                # Comment line

                {key1}="Server=test;User=sa=1"
                export {key2}='single_quoted_val'
                {key3}=SHOULD_NOT_OVERWRITE
                INVALID_LINE_WITHOUT_EQUALS
                """);

            DotEnvLoader.LoadFile(tempFile);

            Assert.AreEqual("Server=test;User=sa=1", Environment.GetEnvironmentVariable(key1));
            Assert.AreEqual("single_quoted_val", Environment.GetEnvironmentVariable(key2));
            Assert.AreEqual("PRE_EXISTING_VALUE", Environment.GetEnvironmentVariable(key3));
        }
        finally
        {
            Environment.SetEnvironmentVariable(key1, null);
            Environment.SetEnvironmentVariable(key2, null);
            Environment.SetEnvironmentVariable(key3, null);
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public async Task SqlDatabaseBootstrapper_HandlesGoWithCommentsAndMissingFile()
    {
        string script = """
            SELECT 1;
            go -- batch 1 end
            SELECT 2;
            GO
            """;

        string[] batches = SqlDatabaseBootstrapper.SplitSqlBatches(script);
        Assert.AreEqual(2, batches.Length);
        Assert.AreEqual(0, SqlDatabaseBootstrapper.SplitSqlBatches("   ").Length);

        var bootstrapper = new SqlDatabaseBootstrapper(Options.Create(new DatabaseOptions
        {
            ConnectionString = "Server=localhost;Database=Test;",
            CommandTimeoutSeconds = 10
        }));

        var missingRes = await bootstrapper.ExecuteSetupScriptAsync(Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid():N}.sql"));
        Assert.IsTrue(missingRes.IsFailure);
        Assert.AreEqual("SQL_SCRIPT_NOT_FOUND", missingRes.Error.Code);
    }

    [TestMethod]
    public async Task EfCoreSyntheticPortfolioGenerator_HandlesGapsInCustomerExternalNoWithoutKeyCollision()
    {
        var options = new DbContextOptionsBuilder<CausalCampaignDbContext>()
            .UseInMemoryDatabase(nameof(EfCoreSyntheticPortfolioGenerator_HandlesGapsInCustomerExternalNoWithoutKeyCollision))
            .Options;

        using var db = new CausalCampaignDbContext(options);
        // Simulate a portfolio where CUST-1001..CUST-1004 were deleted and only CUST-1005 exists
        db.AddEntity(new CustomerProfileMis
        {
            CustomerExternalNo = "CUST-1005",
            FullName = "Existing Customer",
            PrestigeSegment = "Gold",
            TotalCreditLimit = 100000m,
            AvailableCreditLimit = 80000m
        });
        await db.SaveChangesAsync();

        var generator = new EfCoreSyntheticPortfolioGenerator(db);

        // Invalid count fails
        var invalidRes = await generator.EnsureSyntheticPortfolioSizeAsync(0);
        Assert.IsTrue(invalidRes.IsFailure);

        // Expand to 10 customers -> should start at CUST-1006, never colliding with CUST-1005
        var seedRes = await generator.EnsureSyntheticPortfolioSizeAsync(10);
        Assert.IsTrue(seedRes.IsSuccess);
        Assert.AreEqual(9, seedRes.Value);

        var allExternalNos = await db.CustomerProfilesSet.Select(c => c.CustomerExternalNo).ToListAsync();
        Assert.AreEqual(10, allExternalNos.Count);
        Assert.AreEqual(10, allExternalNos.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.IsTrue(allExternalNos.Contains("CUST-1006"));

        // Re-running with same target is idempotent (0 newly inserted)
        var secondRun = await generator.EnsureSyntheticPortfolioSizeAsync(10);
        Assert.IsTrue(secondRun.IsSuccess);
        Assert.AreEqual(0, secondRun.Value);
    }
}
