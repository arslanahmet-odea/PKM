using System.ComponentModel.DataAnnotations;
using CausalCampaign.Infrastructure.Configuration;
using CausalCampaign.Infrastructure.Seeding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.InfrastructureTests;

[TestClass]
public class DatabaseOptionsValidationTests
{
    [TestMethod]
    public void DatabaseOptions_WhenConnectionStringIsEmpty_FailsDataAnnotationValidation()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            ConnectionString = string.Empty,
            TablePrefix = "CasualCampaign_",
            CommandTimeoutSeconds = 30
        };
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(DatabaseOptions.ConnectionString))));
    }

    [TestMethod]
    public void DatabaseOptions_WithValidConnectionStringAndPrefix_PassesValidation()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            ConnectionString = "Data Source=OMV3DVPDBLS,1469;Initial Catalog=DB_Odeabank_DVP_Cards;",
            TablePrefix = "CasualCampaign_",
            CommandTimeoutSeconds = 30
        };
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        // Assert
        Assert.IsTrue(isValid);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void SqlDatabaseBootstrapper_SplitSqlBatches_SplitsOnStandaloneGoLinesOnly()
    {
        // Arrange
        string sampleScript = """
            CREATE TABLE dbo.CasualCampaign_CAMPAIGN_TEMPLATE (Id INT);
            GO
            INSERT INTO dbo.CasualCampaign_CAMPAIGN_TEMPLATE VALUES (1);
            GO
            """;

        // Act
        string[] batches = SqlDatabaseBootstrapper.SplitSqlBatches(sampleScript);

        // Assert
        Assert.AreEqual(2, batches.Length);
        Assert.IsTrue(batches[0].Contains("CREATE TABLE dbo.CasualCampaign_CAMPAIGN_TEMPLATE"));
        Assert.IsTrue(batches[1].Contains("INSERT INTO dbo.CasualCampaign_CAMPAIGN_TEMPLATE"));
    }
}
