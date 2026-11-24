using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.ValueObjects;

namespace CausalCampaign.Application.DTOs;

/// <summary>
/// DTO summarizing the 8 KPIs, budget utilization, CUPED, and PSI Drift status for the Back-Office executive dashboard.
/// </summary>
public sealed record ExecutiveDashboardSummaryDto(
    int CampaignId,
    string CampaignCode,
    string CampaignName,
    bool IsActive,
    bool KillSwitchTriggered,
    string? KillSwitchReason,
    decimal TotalBudget,
    decimal ConsumedBudget,
    decimal RemainingBudget,
    int TotalAssignedCustomers,
    int CompletedGoalCustomers,
    int ExcludedAntiGamingTxCount,
    int ClawedBackRewardCount,
    decimal Kpi1_PosMacroF1Score,
    decimal Kpi2_FreeRiderRatio,
    decimal Kpi3_CupedIncrementalRoi,
    decimal Kpi4_DormantReactivationRate,
    decimal Kpi5_AverageTrackerLatencyMs,
    double Kpi6_LastMilpSolveSeconds,
    decimal Kpi7_MonthlyRealPsiDrift,
    int Kpi8_FairnessRuleViolationCount);

/// <summary>
/// DTO carrying a customer's active offer, XAI rationale, 3 equivalent offer options, live progress tracker,
/// and transaction audit trail for the Call Center and Mobile Simulator views.
/// </summary>
public sealed record CustomerCallCenterAuditDto(
    CustomerProfileMis Customer,
    CustomerOfferAssignment? ActiveAssignment,
    OfferProgressTracker? ActiveTracker,
    RewardFulfillmentQueue? LatestRewardStatus,
    IReadOnlyList<AlternativeOfferOption> AlternativeOptions,
    IReadOnlyList<FeatureContributionItem> XaiFeatureContributions,
    IReadOnlyList<TransactionLedgerFeed> RecentTransactions);
