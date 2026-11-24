using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CausalCampaign.Infrastructure.Seeding;

/// <summary>
/// Deterministic-seed (Random(42)) EF Core synthetic customer portfolio generator for R-Learner causal uplift and MILP testing.
/// </summary>
public sealed class EfCoreSyntheticPortfolioGenerator
{
    private readonly ICausalCampaignDbContext _dbContext;

    public EfCoreSyntheticPortfolioGenerator(ICausalCampaignDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Expands the customer portfolio in the database up to the target count across Persuadable, FreeRider_SureThing,
    /// Dormant_Sleeper, and LostCause archetypes.
    /// </summary>
    /// <param name="targetCustomerCount">Target total customer count (e.g., 100 or 1000).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of newly inserted customer records.</returns>
    public async Task<Result<int>> EnsureSyntheticPortfolioSizeAsync(
        int targetCustomerCount,
        CancellationToken cancellationToken = default)
    {
        if (targetCustomerCount <= 0)
        {
            return Result<int>.Failure(DomainError.Validation(
                "INVALID_TARGET_PORTFOLIO_SIZE",
                "Target portfolio size must be greater than zero."));
        }

        var existingExternalNos = await _dbContext.CustomerProfiles
            .Select(c => c.CustomerExternalNo)
            .ToListAsync(cancellationToken);

        int existingCount = existingExternalNos.Count;
        if (existingCount >= targetCustomerCount)
        {
            return Result<int>.Success(0);
        }

        int maxExistingSeq = 1000;
        foreach (string extNo in existingExternalNos)
        {
            if (extNo.StartsWith("CUST-", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(extNo.AsSpan(5), out int parsedSeq) &&
                parsedSeq > maxExistingSeq)
            {
                maxExistingSeq = parsedSeq;
            }
        }

        int toGenerate = targetCustomerCount - existingCount;
        var rng = new Random(42);

        string[] segments = ["Standard", "Silver", "Gold", "Private"];
        string[] personas = ["Persuadable", "Persuadable", "FreeRider_SureThing", "Dormant_Sleeper", "LostCause"];
        string[] firstNames = ["Emre", "Deniz", "Merve", "Kerem", "Selin", "Hakan", "Gizem", "Tolga", "Pinar", "Baris"];
        string[] lastNames = ["Korkmaz", "Yildirim", "Aydin", "Ozdemir", "Arslan", "Dogan", "Kilic", "Cetin", "Koc", "Sen"];

        for (int i = 1; i <= toGenerate; i++)
        {
            int seq = maxExistingSeq + i;
            string externalNo = $"CUST-{seq}";
            string segment = segments[seq % segments.Length];
            string persona = personas[seq % personas.Length];
            bool isDormant = persona == "Dormant_Sleeper";

            decimal totalLimit = segment switch
            {
                "Private" => 350000m,
                "Gold" => 150000m,
                "Silver" => 80000m,
                _ => 40000m
            };

            decimal availableLimit = Math.Round(totalLimit * (decimal)(0.45 + (rng.NextDouble() * 0.50)), 2);
            decimal baselineSpend = isDormant
                ? 0m
                : persona == "FreeRider_SureThing"
                    ? Math.Round(totalLimit * 0.35m, 2)
                    : Math.Round(totalLimit * 0.16m, 2);

            decimal realSpendIndex = Math.Round(baselineSpend * 1.07m, 2);
            decimal misProfit = persona == "LostCause"
                ? -650.00m
                : Math.Round((baselineSpend * 0.04m) + 750.00m, 2);

            var customer = new CustomerProfileMis
            {
                CustomerExternalNo = externalNo,
                FullName = $"{firstNames[seq % firstNames.Length]} {lastNames[(seq / 3) % lastNames.Length]}",
                CardNumberMasked = $"5400 61** **** {seq % 10000:D4}",
                PrestigeSegment = segment,
                TotalCreditLimit = totalLimit,
                AvailableCreditLimit = availableLimit,
                MisMonthlyNetProfit = misProfit,
                HistoricalBaselineSpend = baselineSpend,
                RealBaselineSpendIndex = realSpendIndex,
                SpendElasticityPersona = persona,
                IsDormant = isDormant,
                DaysSinceLastTx = isDormant ? 105 + (seq % 60) : 1 + (seq % 7),
                HasMarketingConsent = true,
                PreferredRewardType = seq % 2 == 0 ? "Cashback" : "RewardPoints",
                BehavioralClusterId = (seq % 4) + 1
            };

            _dbContext.AddEntity(customer);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(toGenerate);
    }
}
