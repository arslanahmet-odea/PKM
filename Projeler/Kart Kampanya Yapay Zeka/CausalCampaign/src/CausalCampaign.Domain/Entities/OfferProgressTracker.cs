using CausalCampaign.Domain.Common;

namespace CausalCampaign.Domain.Entities;

/// <summary>
/// Rich domain entity representing the OFFER_PROGRESS_TRACKER table (&lt; 500 ms real-time spend goal tracker).
/// Encapsulates optimistic concurrency (RowVersion), goal completion, and post-refund Clawback detection logic.
/// </summary>
public class OfferProgressTracker
{
    public int TrackerId { get; set; }
    public int AssignmentId { get; set; }
    public int CustomerId { get; set; }
    public int CampaignId { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentQualifiedSpend { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal ProgressPercentage { get; set; }
    public int QualifiedTxCount { get; set; }
    public int ExcludedTxCount { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastTransactionAt { get; set; }

    /// <summary>
    /// SQL Server ROWVERSION optimistic concurrency token.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Adds a qualified card transaction to the tracker and checks whether the spend target was newly completed.
    /// </summary>
    /// <param name="amount">Qualified transaction amount (TRY).</param>
    /// <param name="timestampUtc">Transaction timestamp in UTC.</param>
    /// <returns>True if the target was completed for the first time with this transaction; otherwise false.</returns>
    public Result<bool> ApplyQualifiedTransaction(decimal amount, DateTime timestampUtc)
    {
        if (amount <= 0m)
        {
            return Result<bool>.Failure(DomainError.Validation(
                "INVALID_TRANSACTION_AMOUNT",
                "Qualified transaction amount to add to the tracker must be greater than zero."));
        }

        if (TargetAmount <= 0m)
        {
            return Result<bool>.Failure(DomainError.Validation(
                "INVALID_TRACKER_TARGET",
                "Spend cannot be incremented on a control group or zero-target tracker."));
        }

        bool wasAlreadyCompleted = IsCompleted;

        CurrentQualifiedSpend = Math.Round(CurrentQualifiedSpend + amount, 2);
        QualifiedTxCount++;
        LastTransactionAt = timestampUtc;

        RecalculateProgressMetrics(timestampUtc);

        bool newlyCompleted = !wasAlreadyCompleted && IsCompleted;
        return Result<bool>.Success(newlyCompleted);
    }

    /// <summary>
    /// Increments the count of transactions excluded by Anti-Gaming rules.
    /// </summary>
    public void IncrementExcludedTransactionCount(DateTime timestampUtc)
    {
        ExcludedTxCount++;
        LastTransactionAt = timestampUtc;
    }

    /// <summary>
    /// Deducts a reversal or refund amount from the tracker and returns whether an earned reward must be clawed back.
    /// </summary>
    /// <param name="refundAmount">Refunded amount (positive value).</param>
    /// <param name="timestampUtc">Refund timestamp in UTC.</param>
    /// <returns>True if a previously completed goal dropped below the target amount after this refund (Clawback required).</returns>
    public Result<bool> ApplyReversalOrRefund(decimal refundAmount, DateTime timestampUtc)
    {
        if (refundAmount <= 0m)
        {
            return Result<bool>.Failure(DomainError.Validation(
                "INVALID_REFUND_AMOUNT",
                "Reversal or refund amount must be greater than zero."));
        }

        bool wasCompletedBeforeRefund = IsCompleted;

        CurrentQualifiedSpend = Math.Max(0m, Math.Round(CurrentQualifiedSpend - refundAmount, 2, MidpointRounding.AwayFromZero));
        QualifiedTxCount = Math.Max(0, QualifiedTxCount - 1);
        LastTransactionAt = timestampUtc;

        RecalculateProgressMetrics(timestampUtc);

        bool requiresClawback = wasCompletedBeforeRefund && !IsCompleted;
        return Result<bool>.Success(requiresClawback);
    }

    /// <summary>
    /// Updates the tracker's target amount when the customer selects an alternative offer option and recalculates progress.
    /// </summary>
    public Result UpdateTargetAmount(decimal newTargetAmount, DateTime timestampUtc)
    {
        if (newTargetAmount <= 0m)
        {
            return Result.Failure(DomainError.Validation(
                "INVALID_NEW_TARGET_AMOUNT",
                "New spend target amount must be greater than zero."));
        }

        TargetAmount = Math.Round(newTargetAmount, 2, MidpointRounding.AwayFromZero);
        RecalculateProgressMetrics(timestampUtc);
        return Result.Success();
    }

    private void RecalculateProgressMetrics(DateTime timestampUtc)
    {
        RemainingAmount = Math.Max(0m, Math.Round(TargetAmount - CurrentQualifiedSpend, 2, MidpointRounding.AwayFromZero));

        if (TargetAmount <= 0m)
        {
            ProgressPercentage = 0.00m;
        }
        else if (CurrentQualifiedSpend >= TargetAmount)
        {
            ProgressPercentage = 100.00m;
        }
        else
        {
            // Never display 100.00% while RemainingAmount > 0
            decimal rawPct = Math.Round((CurrentQualifiedSpend / TargetAmount) * 100.00m, 2, MidpointRounding.AwayFromZero);
            ProgressPercentage = Math.Min(99.99m, rawPct);
        }

        if (CurrentQualifiedSpend >= TargetAmount && TargetAmount > 0m)
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                CompletedAt = timestampUtc;
            }
        }
        else
        {
            IsCompleted = false;
            CompletedAt = null;
        }
    }
}
