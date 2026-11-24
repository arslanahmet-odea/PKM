namespace CausalCampaign.Domain.DTOs;

/// <summary>
/// Real-time ISO 8583 0100/0200 card authorization message request DTO.
/// </summary>
public sealed record AuthorizeTransactionRequestDto(
    string AuthorizationCode,
    int CustomerId,
    int CampaignId,
    decimal Amount,
    string Currency,
    string RawMcc_DE18,
    string RawMerchantName_DE43,
    DateTime TransactionTimestampUtc);

/// <summary>
/// Real-time ISO 8583 0400/0420 reversal or refund request DTO.
/// </summary>
public sealed record ReversalTransactionRequestDto(
    string ReversalAuthCode,
    int CustomerId,
    int CampaignId,
    long OriginalTransactionId,
    decimal RefundAmount,
    DateTime ReversalTimestampUtc);

/// <summary>
/// Request DTO for customer selection among 3 equivalent offers in the mobile app (Active Preference Learning).
/// </summary>
public sealed record SelectAlternativeOfferRequestDto(
    int AssignmentId,
    int CustomerId,
    byte SelectedOfferIndex);

/// <summary>
/// Request DTO for triggering an emergency campaign Kill-Switch.
/// </summary>
public sealed record TriggerKillSwitchRequestDto(
    int CampaignId,
    string Reason,
    string TriggeredBy);

/// <summary>
/// Detailed response DTO for a real-time tracker update, reversal/refund, or Anti-Gaming evaluation.
/// </summary>
public sealed record TrackerUpdateResultDto(
    long TransactionId,
    int TrackerId,
    int CustomerId,
    int CampaignId,
    string ResolvedBrand,
    string ResolvedSector,
    bool IsAcceptedForCampaign,
    bool IsExcludedByAntiGaming,
    string? ExclusionOrClawbackReason,
    decimal TargetAmount,
    decimal CurrentQualifiedSpend,
    decimal RemainingAmount,
    decimal ProgressPercentage,
    bool IsGoalCompleted,
    bool RewardQueuedOrLoaded,
    bool RewardClawedBack,
    decimal ProcessingLatencyMs);
