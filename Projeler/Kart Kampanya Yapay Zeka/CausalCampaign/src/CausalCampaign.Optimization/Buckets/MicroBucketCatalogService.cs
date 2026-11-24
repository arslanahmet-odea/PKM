using CausalCampaign.Domain.DTOs;

namespace CausalCampaign.Optimization.Buckets;

/// <summary>
/// 20 Standard Micro-Bucket Catalog that eliminates operational complexity and structurally guarantees
/// Algorithmic Fairness (Monotonicity: Target_a &gt; Target_b =&gt; Reward_a =&gt; Reward_b).
/// </summary>
public sealed class MicroBucketCatalogService
{
    private static readonly IReadOnlyList<MicroBucketDefinitionDto> StandardBuckets =
    [
        // Buckets 1..4: Dormant Card Reactivation (DormantReactivation)
        new(1,  "Bucket-01: Dormant Card Starter Reactivation (2,500 TRY Target / 150 TRY Cashback)",  "DormantReactivation", 1.00m,  2500.00m,  150.00m, "Cashback"),
        new(2,  "Bucket-02: Dormant Card Standard Reactivation (5,000 TRY Target / 200 TRY Cashback)", "DormantReactivation", 1.00m,  5000.00m,  200.00m, "Cashback"),
        new(3,  "Bucket-03: Dormant Card Gold Reactivation (6,000 TRY Target / 225 TRY Cashback)",     "DormantReactivation", 1.00m,  6000.00m,  225.00m, "Cashback"),
        new(4,  "Bucket-04: Dormant Card Private Reactivation (7,500 TRY Target / 250 TRY Cashback)",  "DormantReactivation", 1.00m,  7500.00m,  250.00m, "Cashback"),

        // Buckets 5..10: Sectoral Cross-Sell (SectoralCrossSell)
        new(5,  "Bucket-05: Sectoral Cross-Sell Tier-1 (10,000 TRY Target / 275 TRY Cashback)",        "SectoralCrossSell",   1.20m, 10000.00m,  275.00m, "Cashback"),
        new(6,  "Bucket-06: Sectoral Cross-Sell Tier-2 (12,500 TRY Target / 300 TRY Cashback)",        "SectoralCrossSell",   1.25m, 12500.00m,  300.00m, "Cashback"),
        new(7,  "Bucket-07: Sectoral Cross-Sell Tier-3 (15,000 TRY Target / 350 TRY Points)",          "SectoralCrossSell",   1.28m, 15000.00m,  350.00m, "RewardPoints"),
        new(8,  "Bucket-08: Sectoral Cross-Sell Tier-4 (18,000 TRY Target / 400 TRY Cashback)",        "SectoralCrossSell",   1.30m, 18000.00m,  400.00m, "Cashback"),
        new(9,  "Bucket-09: Sectoral Cross-Sell Tier-5 (22,000 TRY Target / 450 TRY Cashback)",        "SectoralCrossSell",   1.35m, 22000.00m,  450.00m, "Cashback"),
        new(10, "Bucket-10: Sectoral Cross-Sell Tier-6 (27,500 TRY Target / 550 TRY Points)",          "SectoralCrossSell",   1.40m, 27500.00m,  550.00m, "RewardPoints"),

        // Buckets 11..20: Total Spend Upsell (TotalSpendUpsell)
        new(11, "Bucket-11: Spend Upsell Tier-1 (15,000 TRY Target / 350 TRY Points)",                 "TotalSpendUpsell",    1.22m, 15000.00m,  350.00m, "RewardPoints"),
        new(12, "Bucket-12: Spend Upsell Tier-2 (20,000 TRY Target / 425 TRY Points)",                 "TotalSpendUpsell",    1.25m, 20000.00m,  425.00m, "RewardPoints"),
        new(13, "Bucket-13: Spend Upsell Tier-3 (28,000 TRY Target / 575 TRY Points)",                 "TotalSpendUpsell",    1.28m, 28000.00m,  575.00m, "RewardPoints"),
        new(14, "Bucket-14: Spend Upsell Tier-4 (36,000 TRY Target / 750 TRY Points)",                 "TotalSpendUpsell",    1.32m, 36000.00m,  750.00m, "RewardPoints"),
        new(15, "Bucket-15: Spend Upsell Tier-5 (45,000 TRY Target / 900 TRY Cashback)",               "TotalSpendUpsell",    1.35m, 45000.00m,  900.00m, "Cashback"),
        new(16, "Bucket-16: Spend Upsell Tier-6 (55,000 TRY Target / 1,100 TRY Points)",               "TotalSpendUpsell",    1.38m, 55000.00m, 1100.00m, "RewardPoints"),
        new(17, "Bucket-17: Spend Upsell Tier-7 (65,000 TRY Target / 1,350 TRY Points)",               "TotalSpendUpsell",    1.40m, 65000.00m, 1350.00m, "RewardPoints"),
        new(18, "Bucket-18: Spend Upsell Tier-8 (75,000 TRY Target / 1,600 TRY Cashback)",             "TotalSpendUpsell",    1.42m, 75000.00m, 1600.00m, "Cashback"),
        new(19, "Bucket-19: Spend Upsell Tier-9 (85,000 TRY Target / 1,950 TRY Points)",               "TotalSpendUpsell",    1.45m, 85000.00m, 1950.00m, "RewardPoints"),
        new(20, "Bucket-20: Spend Upsell Tier-10 (95,000 TRY Target / 2,300 TRY Points)",              "TotalSpendUpsell",    1.50m, 95000.00m, 2300.00m, "RewardPoints")
    ];

    /// <summary>
    /// Returns all 20 standard micro-bucket definitions.
    /// </summary>
    public IReadOnlyList<MicroBucketDefinitionDto> GetAllMicroBuckets() => StandardBuckets;

    /// <summary>
    /// Counts assignments in the given list that violate Algorithmic Fairness / Monotonicity
    /// (Target_a &gt; Target_b =&gt; Reward_a &gt;= Reward_b) in O(N log N) time (KPI-8 target: 0 violations).
    /// </summary>
    public static int CountMonotonicityViolations(
        IReadOnlyList<(decimal TargetAmount, decimal RewardAmount)>? activeAssignments)
    {
        if (activeAssignments is null || activeAssignments.Count <= 1)
        {
            return 0;
        }

        // Group by TargetAmount in ascending order; track running max reward seen for strictly smaller targets
        var groupedByTarget = activeAssignments
            .GroupBy(a => a.TargetAmount)
            .OrderBy(g => g.Key)
            .ToList();

        int violations = 0;
        decimal maxRewardForSmallerTargets = decimal.MinValue;

        foreach (var group in groupedByTarget)
        {
            decimal currentGroupMax = decimal.MinValue;
            foreach (var item in group)
            {
                if (maxRewardForSmallerTargets > decimal.MinValue && item.RewardAmount < maxRewardForSmallerTargets)
                {
                    violations++;
                }

                if (item.RewardAmount > currentGroupMax)
                {
                    currentGroupMax = item.RewardAmount;
                }
            }

            if (currentGroupMax > maxRewardForSmallerTargets)
            {
                maxRewardForSmallerTargets = currentGroupMax;
            }
        }

        return violations;
    }
}
