using CausalCampaign.Domain.ValueObjects;

namespace CausalCampaign.Domain.DTOs;

/// <summary>
/// Per-customer prediction output of the Robinson R-Learner causal inference, sector recommendation, and XAI engine.
/// </summary>
public sealed record CustomerCausalPredictionDto(
    int CustomerId,
    string CustomerExternalNo,
    decimal ExpectedOrganicSpend,
    decimal PredictedIncrementalUplift,
    decimal PropensityScore,
    decimal ReactivationProbability,
    string PredictedPersona,
    string RecommendedSector,
    string PreferredRewardType,
    string XaiReasonText,
    IReadOnlyList<FeatureContributionItem> TopFeatureContributions);

/// <summary>
/// Stratified RCT and CUPED (Controlled-experiment Using Pre-Experiment Data) evaluation result for KPI-3 verification.
/// </summary>
public sealed record CupedEvaluationResultDto(
    int CampaignId,
    int TreatmentCustomerCount,
    int ControlCustomerCount,
    decimal ThetaCovariateWeight,
    decimal VarianceReductionPercentage,
    decimal CupedIncrementalSpendPerCustomer,
    decimal TotalIncrementalNetProfit,
    decimal TotalRewardCost,
    decimal IncrementalRoi,
    bool MeetsKpi3Threshold);

/// <summary>
/// Monthly inflationary data drift (Population Stability Index - PSI) monitoring report for KPI-7 verification.
/// </summary>
public sealed record PsiDriftReportDto(
    int ReferenceYearMonth,
    int CurrentYearMonth,
    decimal NominalSpendPsi,
    decimal InflationAdjustedRealSpendPsi,
    bool IsDriftWithinSafeThreshold,
    string DiagnosticSummary);
