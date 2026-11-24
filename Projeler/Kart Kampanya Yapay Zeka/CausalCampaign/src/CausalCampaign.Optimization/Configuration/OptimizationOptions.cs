namespace CausalCampaign.Optimization.Configuration;

/// <summary>
/// Configuration options for the Google.OrTools MILP portfolio optimizer and experiment hold-out splitter.
/// </summary>
public sealed class OptimizationOptions
{
    public const string SectionName = "Optimization";

    /// <summary>
    /// Google.OrTools mixed-integer linear programming solver engine ("SCIP" or "CBC_MIXED_INTEGER_PROGRAMMING").
    /// </summary>
    public string SolverEngine { get; set; } = "SCIP";

    /// <summary>
    /// Stratified Randomized Controlled Trial (Stratified Hold-out RCT) control group ratio (10% = 0.10).
    /// </summary>
    public decimal HoldoutControlGroupRatio { get; set; } = 0.10m;

    /// <summary>
    /// Maximum fraction of a customer's available credit card limit that can be assigned as a spend target (80% = 0.80).
    /// </summary>
    public decimal MaxAvailableCreditLimitRatio { get; set; } = 0.80m;

    /// <summary>
    /// Weight multiplier applied to the customer's bank-wide MIS monthly net profitability in the objective function.
    /// </summary>
    public decimal MisProfitabilityObjectiveWeight { get; set; } = 0.015m;
}
