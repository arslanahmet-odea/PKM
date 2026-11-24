using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CausalCampaign.CausalAi.FeatureStore;

/// <summary>
/// Feature Store service managing inflation-adjusted customer feature vectors (vw_CasualCampaign_CustomerCausalFeatures)
/// and monthly sectoral deflator multipliers (CasualCampaign_INFLATION_INDEX_MONTHLY).
/// </summary>
public sealed class InflationAdjustedFeatureStore
{
    private readonly ICausalCampaignDbContext _dbContext;

    public InflationAdjustedFeatureStore(ICausalCampaignDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves inflation-adjusted causal feature vectors across the entire customer portfolio.
    /// </summary>
    public async Task<Result<IReadOnlyList<CustomerCausalFeatureView>>> GetPortfolioCausalFeaturesAsync(
        CancellationToken cancellationToken = default)
    {
        var features = await _dbContext.CustomerCausalFeatures
            .OrderBy(c => c.CustomerId)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CustomerCausalFeatureView>>.Success(features);
    }

    /// <summary>
    /// Deflates a nominal spend amount from a given period (YYYYMM) and sector into current real purchasing power (real TRY).
    /// </summary>
    public static decimal DeflateNominalToRealSpend(
        decimal nominalSpend,
        int yearMonth,
        string sectorName,
        IReadOnlyList<InflationIndexMonthly>? inflationIndices)
    {
        if (nominalSpend <= 0m)
        {
            return 0m;
        }

        if (inflationIndices is null || inflationIndices.Count == 0)
        {
            return Math.Round(nominalSpend, 2);
        }

        var match = inflationIndices.FirstOrDefault(i =>
                        i.YearMonth == yearMonth &&
                        string.Equals(i.SectorName, sectorName, StringComparison.OrdinalIgnoreCase))
                    ?? inflationIndices.FirstOrDefault(i =>
                        i.YearMonth == yearMonth &&
                        (string.Equals(i.SectorName, "General", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(i.SectorName, "Genel", StringComparison.OrdinalIgnoreCase)));

        decimal multiplier = match is not null && match.DeflatorMultiplier > 0m
            ? match.DeflatorMultiplier
            : 1.000000m;
        return Math.Round(nominalSpend * multiplier, 2);
    }
}
