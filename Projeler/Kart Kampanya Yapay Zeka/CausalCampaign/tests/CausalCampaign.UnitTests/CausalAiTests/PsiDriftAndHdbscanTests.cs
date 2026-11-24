using CausalCampaign.CausalAi.Analytics;
using CausalCampaign.CausalAi.FeatureStore;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.CausalAiTests;

[TestClass]
public class PsiDriftAndHdbscanTests
{
    [TestMethod]
    public void CalculateMonthlyPsiDrift_AfterInflationDeflation_KeepsRealPsiBelowZeroPointFifteenKpi7Threshold()
    {
        // Arrange
        var monitor = new InflationPsiDriftMonitor();
        var inflationIndices = new List<InflationIndexMonthly>
        {
            new() { YearMonth = 202606, SectorName = "General", CpiIndexValue = 100.0m, DeflatorMultiplier = 1.200000m },
            new() { YearMonth = 202611, SectorName = "General", CpiIndexValue = 120.0m, DeflatorMultiplier = 1.000000m }
        };

        // 202606 reference period nominal spends
        decimal[] refNominal = [10000m, 12000m, 15000m, 18000m, 22000m, 26000m, 30000m, 35000m, 42000m, 50000m];
        // 202611 current period nominal spends (nominal drift due to 20% inflation, but real purchasing power is unchanged)
        decimal[] curNominal = [12100m, 14300m, 18100m, 21500m, 26300m, 31100m, 36200m, 41900m, 50200m, 59800m];

        decimal[] refReal = refNominal
            .Select(s => InflationAdjustedFeatureStore.DeflateNominalToRealSpend(s, 202606, "General", inflationIndices))
            .ToArray();
        decimal[] curReal = curNominal
            .Select(s => InflationAdjustedFeatureStore.DeflateNominalToRealSpend(s, 202611, "General", inflationIndices))
            .ToArray();

        // Act
        var driftReport = monitor.CalculateMonthlyPsiDrift(202606, 202611, refNominal, curNominal, refReal, curReal);

        // Assert
        Assert.IsTrue(driftReport.IsSuccess);
        Assert.IsTrue(driftReport.Value.IsDriftWithinSafeThreshold);
        Assert.IsTrue(
            driftReport.Value.InflationAdjustedRealSpendPsi < KpiThresholds.MaxMonthlyPsiDrift,
            $"KPI-7 Real PSI ({driftReport.Value.InflationAdjustedRealSpendPsi}) must be below threshold ({KpiThresholds.MaxMonthlyPsiDrift}).");
    }

    [TestMethod]
    public void BehavioralHdbscanClusterer_DiscoversFourDistinctMicroClustersWithAnalystSummaries()
    {
        // Arrange
        var clusterer = new BehavioralHdbscanClusterer();
        var customers = new List<CustomerCausalFeatureView>
        {
            new() { CustomerId = 1, RealBaselineSpendIndex = 25000m, AvailableCreditLimit = 90000m, DistinctSectorCount = 4, SpendElasticityPersona = "Persuadable" },
            new() { CustomerId = 2, RealBaselineSpendIndex = 14000m, AvailableCreditLimit = 50000m, DistinctSectorCount = 1, SpendElasticityPersona = "Persuadable" },
            new() { CustomerId = 3, RealBaselineSpendIndex = 65000m, AvailableCreditLimit = 200000m, DistinctSectorCount = 5, SpendElasticityPersona = "FreeRider_SureThing" },
            new() { CustomerId = 4, RealBaselineSpendIndex = 0m, AvailableCreditLimit = 120000m, IsDormant = true, DaysSinceLastTx = 120, SpendElasticityPersona = "Dormant_Sleeper" }
        };

        // Act
        var clusters = clusterer.DiscoverBehavioralClusters(customers);

        // Assert
        Assert.AreEqual(4, clusters.Count);
        Assert.IsTrue(clusters.All(c => !string.IsNullOrWhiteSpace(c.AnalystOpportunitySummary)));
    }
}
