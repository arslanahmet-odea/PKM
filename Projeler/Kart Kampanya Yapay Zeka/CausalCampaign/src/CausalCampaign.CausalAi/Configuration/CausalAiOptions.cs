namespace CausalCampaign.CausalAi.Configuration;

/// <summary>
/// Hyperparameters for Robinson R-Learner causal inference, Empirical Bayes shrinkage, and PSI drift monitoring.
/// </summary>
public sealed class CausalAiOptions
{
    public const string SectionName = "CausalAi";

    /// <summary>
    /// Minimum propensity score clipping boundary to prevent division by zero in R-Learner pseudo-outcomes (Y_res / W_res).
    /// </summary>
    public decimal PropensityClipMin { get; set; } = 0.05m;

    /// <summary>
    /// Maximum propensity score clipping boundary in R-Learner pseudo-outcome calculation.
    /// </summary>
    public decimal PropensityClipMax { get; set; } = 0.95m;

    /// <summary>
    /// Empirical Bayes Shrinkage prior strength constant (kappa).
    /// </summary>
    public decimal EmpiricalBayesPriorKappa { get; set; } = 6.0m;

    /// <summary>
    /// Minimum expected organic baseline spend (TRY) required to classify a customer as a Free-Rider (Sure Thing).
    /// </summary>
    public decimal FreeRiderOrganicSpendThreshold { get; set; } = 42_000.00m;

    /// <summary>
    /// Maximum marginal uplift-to-organic-spend ratio for classifying a customer as a Free-Rider.
    /// </summary>
    public decimal FreeRiderMaxRelativeUpliftRatio { get; set; } = 0.08m;

    /// <summary>
    /// KPI-7 monthly Population Stability Index (PSI) maximum safe drift threshold.
    /// </summary>
    public decimal PsiAlertThreshold { get; set; } = 0.15m;
}
