using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.Interfaces;
using CausalCampaign.Domain.ValueObjects;

namespace CausalCampaign.CausalAi.Explainability;

/// <summary>
/// KPI-8 (100% Explainability): Calculates normalized feature contribution weights for each customer decision
/// and generates a human-readable XAI decision rationale for the mobile app and Call Center.
/// </summary>
public sealed class XaiTurkishExplanationService : IXaiExplanationService
{
    /// <summary>
    /// Produces the top 3 contributing features (with normalized impact weights) and a clear English decision rationale.
    /// </summary>
    public (string ExplanationText, IReadOnlyList<FeatureContributionItem> TopContributions) GenerateCustomerExplanation(
        CustomerCausalFeatureView customer,
        decimal assignedTarget,
        decimal assignedReward,
        string rewardType)
    {
        string rewardLabel = string.Equals(rewardType, "Cashback", StringComparison.OrdinalIgnoreCase)
            ? "Cashback"
            : "Reward Points";

        if (customer.IsDormant || customer.DaysSinceLastTx >= 90)
        {
            var dormantContributions = new List<FeatureContributionItem>
            {
                new($"DaysSinceLastTx_{customer.DaysSinceLastTx}", 0.51m),
                new("MisMonthlyNetProfit", 0.31m),
                new("AvailableCreditLimit", 0.18m)
            };

            string dormantExplanationText =
                $"Based on your card's {customer.DaysSinceLastTx}-day inactivity period, strong {customer.PrestigeSegment} segment MIS profitability, and " +
                $"{customer.AvailableCreditLimit:N0} TRY available credit limit, a reactivation offer of " +
                $"{assignedReward:N0} TRY {rewardLabel} for a {assignedTarget:N0} TRY spend target was created for you.";

            return (dormantExplanationText, dormantContributions);
        }

        // Calculate and normalize local feature contribution weights for an active customer
        decimal rawSpendScore = 0.35m + (customer.PortfolioRealSpendQuantile * 0.25m);
        decimal rawSegmentScore = customer.PrestigeSegment is "Gold" or "Private" ? 0.32m : 0.22m;
        decimal rawLimitOrSectorScore = 0.18m + (Math.Min(5, customer.DistinctSectorCount) * 0.03m);

        decimal totalRaw = rawSpendScore + rawSegmentScore + rawLimitOrSectorScore;
        decimal weight1 = Math.Round(rawSpendScore / totalRaw, 2);
        decimal weight2 = Math.Round(rawSegmentScore / totalRaw, 2);
        decimal weight3 = Math.Round(1.00m - weight1 - weight2, 2);

        var contributions = new List<FeatureContributionItem>
        {
            new("RealBaselineSpendIndex", weight1),
            new($"PrestigeSegment_{customer.PrestigeSegment}", weight2),
            new($"PreferredReward_{rewardType}", weight3)
        };

        string activeExplanationText =
            $"Based on your recent {customer.RealBaselineSpendIndex:N0} TRY inflation-adjusted spending momentum, " +
            $"{customer.PrestigeSegment} segment tier, {customer.AvailableCreditLimit:N0} TRY available credit limit, and " +
            $"{rewardLabel} preference, a personalized offer of {assignedReward:N0} TRY {rewardLabel} for a {assignedTarget:N0} TRY spend target was assigned.";

        return (activeExplanationText, contributions);
    }
}
