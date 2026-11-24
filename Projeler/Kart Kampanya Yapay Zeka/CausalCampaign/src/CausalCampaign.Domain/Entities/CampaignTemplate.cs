using CausalCampaign.Domain.Common;

namespace CausalCampaign.Domain.Entities;

/// <summary>
/// Rich domain entity representing the CAMPAIGN_TEMPLATE table.
/// Encapsulates campaign budget allocation, constraints, and emergency Kill-Switch behaviors.
/// </summary>
public class CampaignTemplate : BaseEntity
{
    public int CampaignId { get; set; }
    public string CampaignCode { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;
    public string CampaignType { get; set; } = "TotalSpendUpsell";
    public string TargetSector { get; set; } = "All";
    public string DefaultRewardType { get; set; } = "RewardPoints";
    public decimal TotalBudget { get; set; }
    public decimal ConsumedBudget { get; set; }
    public decimal MaxRewardPerCustomer { get; set; }
    public decimal MinMisProfitThreshold { get; set; } = -500.00m;
    public decimal InterchangeMarginRate { get; set; } = 0.0250m;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool KillSwitchTriggered { get; set; }
    public string? KillSwitchReason { get; set; }
    public string CreatedBy { get; set; } = "SYSTEM_ARCHITECT";

    /// <summary>
    /// SQL Server ROWVERSION optimistic concurrency token protecting budget allocations and Kill-Switch state.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>Calculates the remaining available campaign reward budget.</summary>
    public decimal RemainingBudget => Math.Max(0m, TotalBudget - ConsumedBudget);

    /// <summary>
    /// Verifies that the campaign is active, within its valid date range, and not halted by a Kill-Switch.
    /// </summary>
    public Result VerifyOperationalStatus(DateTime currentUtc)
    {
        if (KillSwitchTriggered)
        {
            return Result.Failure(DomainError.KillSwitch(
                "CAMPAIGN_KILL_SWITCH_ACTIVE",
                $"Campaign ({CampaignCode}) is disabled due to an active emergency Kill-Switch. Reason: {KillSwitchReason ?? "Unspecified"}"));
        }

        if (!IsActive)
        {
            return Result.Failure(DomainError.Validation(
                "CAMPAIGN_INACTIVE",
                $"Campaign ({CampaignCode}) is not currently active."));
        }

        if (currentUtc < StartDate || currentUtc > EndDate)
        {
            return Result.Failure(DomainError.Validation(
                "CAMPAIGN_OUT_OF_DATE_RANGE",
                $"Transaction date is outside the valid date range ({StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd}) for campaign ({CampaignCode})."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates that the campaign is active/unhalted and the requested reward amount does not exceed the per-customer cap or remaining total budget, then allocates it.
    /// </summary>
    public Result AllocateRewardBudget(decimal rewardAmount)
    {
        if (KillSwitchTriggered)
        {
            return Result.Failure(DomainError.KillSwitch(
                "CAMPAIGN_KILL_SWITCH_ACTIVE",
                $"Cannot allocate reward budget while emergency Kill-Switch is active for campaign ({CampaignCode})."));
        }

        if (!IsActive)
        {
            return Result.Failure(DomainError.Validation(
                "CAMPAIGN_INACTIVE",
                $"Cannot allocate reward budget for inactive campaign ({CampaignCode})."));
        }

        decimal roundedReward = Math.Round(rewardAmount, 2, MidpointRounding.AwayFromZero);
        if (roundedReward <= 0m)
        {
            return Result.Failure(DomainError.Validation(
                "INVALID_REWARD_AMOUNT",
                "Reward amount to allocate must be greater than zero."));
        }

        if (roundedReward > MaxRewardPerCustomer)
        {
            return Result.Failure(DomainError.Validation(
                "REWARD_EXCEEDS_CUSTOMER_CAP",
                $"Reward amount ({roundedReward:N2} TRY) cannot exceed the per-customer cap ({MaxRewardPerCustomer:N2} TRY)."));
        }

        if (ConsumedBudget + roundedReward > TotalBudget)
        {
            return Result.Failure(DomainError.Conflict(
                "CAMPAIGN_BUDGET_EXHAUSTED",
                $"Total campaign budget ({TotalBudget:N2} TRY) is insufficient. Remaining budget: {RemainingBudget:N2} TRY."));
        }

        ConsumedBudget = Math.Round(ConsumedBudget + roundedReward, 2, MidpointRounding.AwayFromZero);
        return Result.Success();
    }

    /// <summary>
    /// Releases previously allocated reward budget back to the campaign pool when a reward is clawed back due to a refund.
    /// </summary>
    public Result ReleaseRewardBudget(decimal rewardAmount)
    {
        decimal roundedReward = Math.Round(rewardAmount, 2, MidpointRounding.AwayFromZero);
        if (roundedReward <= 0m)
        {
            return Result.Failure(DomainError.Validation(
                "INVALID_REWARD_AMOUNT",
                "Reward amount to release must be greater than zero."));
        }

        ConsumedBudget = Math.Max(0m, Math.Round(ConsumedBudget - roundedReward, 2, MidpointRounding.AwayFromZero));
        return Result.Success();
    }

    /// <summary>
    /// Activates the emergency Kill-Switch for the campaign and halts further progress or reward allocations.
    /// </summary>
    public Result TriggerKillSwitch(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 5)
        {
            return Result.Failure(DomainError.Validation(
                "KILL_SWITCH_REASON_REQUIRED",
                "A valid reason of at least 5 characters is required to trigger the Kill-Switch."));
        }

        string trimmedReason = reason.Trim();
        if (trimmedReason.Length > 300)
        {
            return Result.Failure(DomainError.Validation(
                "KILL_SWITCH_REASON_TOO_LONG",
                "Kill-Switch reason cannot exceed 300 characters."));
        }

        KillSwitchTriggered = true;
        IsActive = false;
        KillSwitchReason = trimmedReason;
        return Result.Success();
    }
}
