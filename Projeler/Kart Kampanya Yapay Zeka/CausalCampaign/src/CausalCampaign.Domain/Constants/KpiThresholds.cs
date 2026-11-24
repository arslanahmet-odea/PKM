namespace CausalCampaign.Domain.Constants;

/// <summary>
/// Target constants for the 8 Key Performance Indicators (KPIs) verified across the project.
/// </summary>
public static class KpiThresholds
{
    /// <summary>KPI-1: POS Transaction Text (DE-43 &amp; MCC) Classification Accuracy (Macro F1-Score =&gt; 0.92).</summary>
    public const decimal MinPosClassificationMacroF1 = 0.92m;

    /// <summary>KPI-2: Maximum allowable Free-Rider (reward paid to organic spenders) ratio (&lt;= 0.25).</summary>
    public const decimal MaxFreeRiderRatio = 0.25m;

    /// <summary>KPI-3: Minimum Net Incremental Economic Return per unit reward cost (Incremental ROI =&gt; 1.35x).</summary>
    public const decimal MinIncrementalRoi = 1.35m;

    /// <summary>KPI-4: Minimum dormant credit card reactivation conversion rate (=&gt; 0.065).</summary>
    public const decimal MinDormantReactivationRate = 0.065m;

    /// <summary>KPI-5: Maximum P95 end-to-end latency for real-time offer progress tracker updates (&lt; 500 ms).</summary>
    public const decimal MaxRealtimeTrackerLatencyMs = 500.0m;

    /// <summary>KPI-6: Maximum portfolio-wide constrained MILP optimization solve duration (&lt; 15 minutes = 900 seconds).</summary>
    public const double MaxMilpSolveDurationSeconds = 900.0;

    /// <summary>KPI-7: Maximum monthly inflationary Population Stability Index (PSI &lt; 0.15).</summary>
    public const decimal MaxMonthlyPsiDrift = 0.15m;

    /// <summary>KPI-8: Maximum allowable fairness monotonicity or rule violation count (0).</summary>
    public const int MaxAllowedFairnessRuleViolations = 0;
}
