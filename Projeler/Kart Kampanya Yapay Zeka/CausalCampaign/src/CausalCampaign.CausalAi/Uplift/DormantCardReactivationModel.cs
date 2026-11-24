using CausalCampaign.Domain.Entities;

namespace CausalCampaign.CausalAi.Uplift;

/// <summary>
/// Estimates the 60-day sustained reactivation probability (KPI-4: Reactivation =&gt; 6.5%)
/// and expected incremental spend for dormant credit cards inactive for 90+ days.
/// </summary>
public sealed class DormantCardReactivationModel
{
    /// <summary>
    /// Predicts the reactivation probability and expected 60-day incremental spend for a dormant cardholder.
    /// </summary>
    public (decimal ReactivationProbability, decimal PredictedReactivationUpliftSpend) PredictDormantReactivation(
        CustomerCausalFeatureView customer)
    {
        if (!customer.IsDormant && customer.DaysSinceLastTx < 90)
        {
            return (0.0000m, 0.00m);
        }

        // Logistic activation model: MIS profitability, available limit, and prestige tier contribute positively;
        // prolonged inactivity (180+ days) gradually decays probability.
        double segmentBonus = customer.PrestigeSegment switch
        {
            "Private" => 0.85,
            "Gold" => 0.65,
            "Silver" => 0.35,
            _ => 0.10
        };

        double misFactor = Math.Clamp((double)customer.MisMonthlyNetProfit / 4000.0, -0.50, 1.20);
        double limitFactor = Math.Clamp((double)customer.AvailableCreditLimit / 100000.0, 0.10, 1.00);
        double recencyDecay = Math.Clamp((customer.DaysSinceLastTx - 90) / 180.0, 0.0, 1.20);

        double logit = -2.25 + segmentBonus + (0.60 * misFactor) + (0.45 * limitFactor) - (0.55 * recencyDecay);
        double probability = 1.0 / (1.0 + Math.Exp(-logit));

        decimal clampedProb = Math.Round((decimal)Math.Clamp(probability, 0.025, 0.450), 4);

        decimal baseReactivationSpend = customer.PrestigeSegment is "Gold" or "Private" ? 7200.00m : 4800.00m;
        decimal expectedUplift = Math.Round(baseReactivationSpend * (0.65m + clampedProb), 2);

        return (clampedProb, expectedUplift);
    }
}
