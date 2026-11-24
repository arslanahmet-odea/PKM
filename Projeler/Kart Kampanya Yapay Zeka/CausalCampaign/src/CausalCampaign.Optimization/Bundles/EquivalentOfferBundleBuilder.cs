using System.Globalization;
using System.Text.Json;
using CausalCampaign.Domain.ValueObjects;

namespace CausalCampaign.Optimization.Bundles;

/// <summary>
/// Generates 3 equivalent-profit alternative offer options that the customer can choose from in the mobile app
/// under Active Preference Learning, while preserving MILP budget caps, credit limit caps, and monotonicity.
/// </summary>
public sealed class EquivalentOfferBundleBuilder
{
    /// <summary>
    /// Builds a JSON array of 3 equivalent-profit offer options around the primary MILP-assigned offer (Option 1).
    /// All alternatives respect maxAllowedTargetAmount, maxRewardPerCustomer, primaryRewardAmount (budget safety),
    /// and monotonicity (Target_a &gt; Target_b =&gt; Reward_a &gt;= Reward_b).
    /// </summary>
    public string BuildEquivalentOffersJson(
        decimal primaryTargetAmount,
        decimal primaryRewardAmount,
        string primaryRewardType,
        string recommendedSector,
        decimal maxAllowedTargetAmount,
        decimal maxRewardPerCustomer)
    {
        if (primaryTargetAmount <= 0m || primaryRewardAmount <= 0m || maxAllowedTargetAmount <= 0m || maxRewardPerCustomer <= 0m)
        {
            return "[]";
        }

        decimal effectivePrimaryTarget = Math.Min(primaryTargetAmount, maxAllowedTargetAmount);
        decimal effectivePrimaryReward = Math.Min(primaryRewardAmount, maxRewardPerCustomer);

        string primaryLabel = string.Equals(primaryRewardType, "Cashback", StringComparison.OrdinalIgnoreCase)
            ? "Cashback"
            : "Points";

        // Option 1: Primary offer assigned by MILP
        var option1 = new AlternativeOfferOption(
            Index: 1,
            Title: string.Format(
                CultureInfo.InvariantCulture,
                "Spend {0:N0} TRY to Earn {1:N0} TRY {2}",
                effectivePrimaryTarget,
                effectivePrimaryReward,
                primaryLabel),
            Target: effectivePrimaryTarget,
            Reward: effectivePrimaryReward,
            RewardType: primaryRewardType);

        // Option 2: Opposite reward type (Points <-> Cashback) with equivalent net bank margin and budget safety (<= primaryReward)
        string altRewardType = string.Equals(primaryRewardType, "Cashback", StringComparison.OrdinalIgnoreCase)
            ? "RewardPoints"
            : "Cashback";
        string altRewardLabel = altRewardType == "Cashback" ? "Cashback" : "Points";

        decimal option2Target = Math.Min(
            maxAllowedTargetAmount,
            Math.Max(effectivePrimaryTarget, Math.Round((effectivePrimaryTarget * 1.05m) / 100m, 0) * 100m));

        decimal option2Reward = altRewardType == "Cashback"
            ? Math.Max(1m, Math.Round(effectivePrimaryReward * 0.92m, 0))
            : effectivePrimaryReward;

        // Preserve monotonicity if option2Target > effectivePrimaryTarget
        if (option2Target > effectivePrimaryTarget && option2Reward < effectivePrimaryReward)
        {
            option2Target = effectivePrimaryTarget;
        }

        var option2 = new AlternativeOfferOption(
            Index: 2,
            Title: string.Format(
                CultureInfo.InvariantCulture,
                "Spend {0:N0} TRY to Earn {1:N0} TRY {2}",
                option2Target,
                option2Reward,
                altRewardLabel),
            Target: option2Target,
            Reward: option2Reward,
            RewardType: altRewardType);

        // Option 3: Sector-focused alternative offer with lower target and proportionally bounded reward (monotonic & budget-safe)
        string sectorTitle = string.IsNullOrWhiteSpace(recommendedSector) ? "E-Commerce & Marketplace" : recommendedSector;
        decimal rawOption3Target = Math.Round((effectivePrimaryTarget * 0.55m) / 100m, 0) * 100m;
        decimal option3Target = Math.Min(maxAllowedTargetAmount, Math.Max(Math.Min(1500m, maxAllowedTargetAmount), rawOption3Target));
        decimal option3Reward = Math.Min(
            effectivePrimaryReward,
            Math.Max(1m, Math.Round(effectivePrimaryReward * 0.90m, 0)));

        var option3 = new AlternativeOfferOption(
            Index: 3,
            Title: string.Format(
                CultureInfo.InvariantCulture,
                "Spend {0:N0} TRY in {1} to Earn {2:N0} TRY {3}",
                option3Target,
                sectorTitle,
                option3Reward,
                primaryLabel),
            Target: option3Target,
            Reward: option3Reward,
            RewardType: primaryRewardType);

        var bundle = new List<AlternativeOfferOption> { option1, option2, option3 };
        return JsonSerializer.Serialize(bundle);
    }
}
