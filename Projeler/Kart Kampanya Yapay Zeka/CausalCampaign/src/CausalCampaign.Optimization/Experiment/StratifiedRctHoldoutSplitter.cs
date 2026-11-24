using CausalCampaign.Domain.Entities;

namespace CausalCampaign.Optimization.Experiment;

/// <summary>
/// Stratifies eligible customers by spend decile (SpendDecile) and prestige tier (PrestigeSegment)
/// into 90% Treatment and 10% Control (Control_Holdout) cohorts for CUPED net lift evaluation.
/// </summary>
public sealed class StratifiedRctHoldoutSplitter
{
    /// <summary>
    /// Selects customer IDs into the stratified control (Hold-out) cohort.
    /// </summary>
    /// <param name="eligibleCustomers">Customers eligible to receive an offer.</param>
    /// <param name="holdoutRatio">Control group ratio (default 0.10 = 10%).</param>
    /// <returns>Set of customer IDs assigned to the Control_Holdout group.</returns>
    public HashSet<int> SelectStratifiedControlGroupCustomerIds(
        IReadOnlyList<CustomerCausalFeatureView> eligibleCustomers,
        decimal holdoutRatio = 0.10m)
    {
        var holdoutIds = new HashSet<int>();
        if (eligibleCustomers is null || eligibleCustomers.Count <= 1 || holdoutRatio <= 0m || holdoutRatio >= 1m)
        {
            return holdoutIds;
        }

        int targetHoldoutCount = Math.Clamp(
            (int)Math.Round(eligibleCustomers.Count * holdoutRatio, MidpointRounding.AwayFromZero),
            1,
            eligibleCustomers.Count - 1);

        var orderedStrata = eligibleCustomers
            .OrderBy(c => c.PrestigeSegment, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.SpendDecile)
            .ThenBy(c => c.CustomerId)
            .ToList();

        // Evenly distribute hold-out indices across the entire stratified list (including tail strata)
        double stride = (double)orderedStrata.Count / targetHoldoutCount;
        for (int k = 0; k < targetHoldoutCount; k++)
        {
            int idx = Math.Clamp((int)Math.Floor((k + 0.5) * stride), 0, orderedStrata.Count - 1);
            holdoutIds.Add(orderedStrata[idx].CustomerId);
        }

        return holdoutIds;
    }
}
