using CausalCampaign.Domain.Common;

namespace CausalCampaign.Domain.Entities;

/// <summary>
/// Rich domain entity representing the REWARD_FULFILLMENT_QUEUE table for reward loading and refund-induced Clawback.
/// </summary>
public class RewardFulfillmentQueue
{
    public int FulfillmentId { get; set; }
    public int TrackerId { get; set; }
    public int CustomerId { get; set; }
    public int CampaignId { get; set; }
    public decimal RewardAmount { get; set; }
    public string RewardType { get; set; } = "RewardPoints";
    public string Status { get; set; } = "PendingSettlement";
    public DateTime ScheduledLoadDate { get; set; }
    public DateTime? LoadedAt { get; set; }
    public DateTime? ClawedBackAt { get; set; }
    public string? ClawbackReason { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// SQL Server ROWVERSION optimistic concurrency token protecting settlement load vs. refund clawback transitions.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Claws back the reward when total qualified spend drops below the target due to a reversal or refund.
    /// </summary>
    public Result TriggerClawback(string reason, DateTime timestampUtc)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(DomainError.Validation(
                "CLAWBACK_REASON_REQUIRED",
                "A reason is required to execute reward Clawback."));
        }

        if (Status == "ClawedBack")
        {
            return Result.Failure(DomainError.Conflict(
                "REWARD_ALREADY_CLAWED_BACK",
                "This reward entry has already been clawed back."));
        }

        Status = "ClawedBack";
        ClawbackReason = reason.Trim();
        ClawedBackAt = timestampUtc;
        UpdatedAt = timestampUtc;

        return Result.Success();
    }

    /// <summary>
    /// Marks the reward as loaded onto the customer's card after end-of-day clearing and settlement confirmation.
    /// </summary>
    public Result MarkAsLoaded(DateTime timestampUtc)
    {
        if (Status == "ClawedBack")
        {
            return Result.Failure(DomainError.Conflict(
                "CANNOT_LOAD_CLAWED_BACK_REWARD",
                "A clawed-back reward cannot be loaded onto a card."));
        }

        if (Status == "Loaded")
        {
            return Result.Failure(DomainError.Conflict(
                "REWARD_ALREADY_LOADED",
                "This reward entry has already been loaded onto the card."));
        }

        Status = "Loaded";
        LoadedAt = timestampUtc;
        UpdatedAt = timestampUtc;
        return Result.Success();
    }
}
