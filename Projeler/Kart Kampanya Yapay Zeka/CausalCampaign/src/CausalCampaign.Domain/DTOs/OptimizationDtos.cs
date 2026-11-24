namespace CausalCampaign.Domain.DTOs;

/// <summary>
/// Represents one of the 20 standard micro-bucket definitions.
/// </summary>
public sealed record MicroBucketDefinitionDto(
    int MicroBucketId,
    string MicroBucketName,
    string ApplicableCampaignType,
    decimal TargetSpendMultiplier,
    decimal BaseTargetAmount,
    decimal BaseRewardAmount,
    string DefaultRewardType);

/// <summary>
/// Per-customer optimal offer assignment output of the Google.OrTools MILP solver.
/// </summary>
public sealed record CustomerMilpAssignmentDto(
    int CustomerId,
    int MicroBucketId,
    string MicroBucketName,
    string ExperimentGroup,
    decimal AssignedTargetAmount,
    decimal AssignedRewardAmount,
    string RewardType,
    decimal ExpectedNetIncrementalProfit,
    string AlternativeOffersJson);

/// <summary>
/// Portfolio-wide constrained MILP optimization summary and KPI verification output.
/// </summary>
public sealed record PortfolioOptimizationResultDto(
    int CampaignId,
    string SolverStatus,
    int TotalEvaluatedCustomers,
    int AssignedTreatmentCount,
    int AssignedControlHoldoutCount,
    int ExcludedFreeRiderCount,
    decimal FreeRiderRatio,
    decimal TotalAllocatedRewardBudget,
    decimal TotalExpectedIncrementalNetProfit,
    int FairnessMonotonicityViolationCount,
    double SolveDurationSeconds,
    IReadOnlyList<CustomerMilpAssignmentDto> Assignments);
