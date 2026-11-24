using System.Diagnostics;
using CausalCampaign.Application.Validators;
using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace CausalCampaign.Application.Services;

/// <summary>
/// Real-time ISO 8583 authorization tracker, 30-minute split-transaction blocker, cash-like MCC filter,
/// and reversal/refund reward Clawback service achieving KPI-5 (&lt; 500 ms P95 latency) and KPI-8 (0% rule violation).
/// </summary>
public sealed class RealtimeOfferTrackerService : IRealtimeOfferTrackerService
{
    private readonly ICausalCampaignDbContext _dbContext;
    private readonly IPosEnrichmentService _posEnrichmentService;
    private readonly AuthorizeTransactionRequestValidator _authorizeValidator = new();
    private readonly ReversalTransactionRequestValidator _reversalValidator = new();

    public RealtimeOfferTrackerService(
        ICausalCampaignDbContext dbContext,
        IPosEnrichmentService posEnrichmentService)
    {
        _dbContext = dbContext;
        _posEnrichmentService = posEnrichmentService;
    }

    /// <summary>
    /// Processes a live card authorization message, applies 3-layer POS enrichment and Anti-Gaming filters,
    /// atomically updates the spend goal tracker if qualified, and queues the reward upon goal completion.
    /// </summary>
    public async Task<Result<TrackerUpdateResultDto>> ProcessAuthorizationAsync(
        AuthorizeTransactionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // 1. FluentValidation Input Validation
        if (request is null)
        {
            return Result<TrackerUpdateResultDto>.Failure(DomainError.Validation(
                "INVALID_AUTHORIZATION_REQUEST",
                "Authorization request cannot be null."));
        }

        var validation = _authorizeValidator.Validate(request);
        if (!validation.IsValid)
        {
            return Result<TrackerUpdateResultDto>.Failure(DomainError.Validation(
                "INVALID_AUTHORIZATION_REQUEST",
                string.Join(" ", validation.Errors.Select(e => e.ErrorMessage))));
        }

        string trimmedAuthCode = request.AuthorizationCode.Trim();

        // 1B. Idempotency / Replay Protection on AuthorizationCode
        bool isDuplicateAuth = await QueryAnyAsync(
            _dbContext.TransactionLedgerFeeds,
            tx => tx.CustomerId == request.CustomerId
                  && tx.AuthorizationCode == trimmedAuthCode
                  && !tx.IsReversalOrRefund,
            cancellationToken);

        if (isDuplicateAuth)
        {
            return Result<TrackerUpdateResultDto>.Failure(DomainError.Conflict(
                "DUPLICATE_AUTHORIZATION_CODE",
                $"Authorization code ({trimmedAuthCode}) has already been processed for Customer (ID={request.CustomerId})."));
        }

        // 2. Campaign and Active Tracker Verification
        var campaign = await QueryFirstOrDefaultAsync(
            _dbContext.CampaignTemplates,
            c => c.CampaignId == request.CampaignId,
            cancellationToken);

        if (campaign is null)
        {
            return Result<TrackerUpdateResultDto>.Failure(DomainError.NotFound(
                "CAMPAIGN_NOT_FOUND",
                $"Campaign (ID={request.CampaignId}) was not found."));
        }

        var statusCheck = campaign.VerifyOperationalStatus(request.TransactionTimestampUtc);
        if (statusCheck.IsFailure)
        {
            return Result<TrackerUpdateResultDto>.Failure(statusCheck.Error);
        }

        var tracker = await QueryFirstOrDefaultAsync(
            _dbContext.OfferProgressTrackers,
            t => t.CustomerId == request.CustomerId && t.CampaignId == request.CampaignId,
            cancellationToken);

        if (tracker is null)
        {
            return Result<TrackerUpdateResultDto>.Failure(DomainError.NotFound(
                "ACTIVE_TRACKER_NOT_FOUND",
                $"No active spend goal tracker found for Customer (ID={request.CustomerId}) in Campaign (ID={request.CampaignId})."));
        }

        // 3. 3-Layer POS Transaction Enrichment (DE-18 MCC & DE-43 Merchant Name)
        var enrichmentResult = await _posEnrichmentService.EnrichTransactionMerchantAsync(
            new PosEnrichmentRequestDto(request.RawMerchantName_DE43, request.RawMcc_DE18),
            cancellationToken);

        if (enrichmentResult.IsFailure)
        {
            return Result<TrackerUpdateResultDto>.Failure(enrichmentResult.Error);
        }

        var pos = enrichmentResult.Value;
        var txLedger = new TransactionLedgerFeed
        {
            AuthorizationCode = request.AuthorizationCode.Trim(),
            CustomerId = request.CustomerId,
            TransactionTimestamp = request.TransactionTimestampUtc,
            Amount = request.Amount,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            RawMcc_DE18 = request.RawMcc_DE18.Trim(),
            RawMerchantName_DE43 = request.RawMerchantName_DE43.Trim(),
            ResolvedBrand = pos.CleanedBrandName,
            ResolvedSector = pos.ResolvedSector,
            ClassificationLayer = (byte)pos.ClassificationLayer,
            IsReversalOrRefund = false
        };

        // 4. Anti-Gaming Rule 1: Cash-Like or Excluded MCC
        if (pos.IsCashLikeOrExcludedMcc || AntiGamingConstants.IsExcludedMcc(request.RawMcc_DE18))
        {
            string reason = $"Anti-Gaming: Cash-like or excluded MCC code ({request.RawMcc_DE18}).";
            return await RecordExcludedTransactionAsync(txLedger, tracker, reason, isAntiGamingViolation: true, stopwatch, cancellationToken);
        }

        // 5. Anti-Gaming Rule 2: Symmetric Split-Transaction Window [T - 30m, T + 30m] at Same Brand
        bool isGenericFallbackBrand = string.IsNullOrWhiteSpace(pos.CleanedBrandName)
                                      || string.Equals(pos.CleanedBrandName, "UNKNOWN MERCHANT", StringComparison.OrdinalIgnoreCase);

        if (!isGenericFallbackBrand)
        {
            DateTime splitWindowStartUtc = request.TransactionTimestampUtc.AddMinutes(-AntiGamingConstants.SplitTransactionWindowMinutes);
            DateTime splitWindowEndUtc = request.TransactionTimestampUtc.AddMinutes(AntiGamingConstants.SplitTransactionWindowMinutes);
            bool hasRecentSameBrandTx = await QueryAnyAsync(
                _dbContext.TransactionLedgerFeeds,
                tx => tx.CustomerId == request.CustomerId
                      && tx.ResolvedBrand == pos.CleanedBrandName
                      && !tx.IsReversalOrRefund
                      && !tx.IsExcludedByAntiGaming
                      && tx.TransactionTimestamp >= splitWindowStartUtc
                      && tx.TransactionTimestamp <= splitWindowEndUtc,
                cancellationToken);

            if (hasRecentSameBrandTx)
            {
                string reason = $"Anti-Gaming: Split transaction detected at the same merchant ({pos.CleanedBrandName}) within {AntiGamingConstants.SplitTransactionWindowMinutes} minutes.";
                return await RecordExcludedTransactionAsync(txLedger, tracker, reason, isAntiGamingViolation: true, stopwatch, cancellationToken);
            }
        }

        // 6. Sectoral Campaign Eligibility Verification (legitimate non-target sector is NOT an anti-gaming violation)
        if (!string.Equals(campaign.TargetSector, "All", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(campaign.TargetSector, pos.ResolvedSector, StringComparison.OrdinalIgnoreCase))
        {
            string reason = $"Transaction sector ({pos.ResolvedSector}) does not match campaign target sector ({campaign.TargetSector}).";
            return await RecordExcludedTransactionAsync(txLedger, tracker, reason, isAntiGamingViolation: false, stopwatch, cancellationToken);
        }

        // 7. Atomic Tracker Update, Campaign Budget Allocation & Reward Entitlement
        var applyResult = tracker.ApplyQualifiedTransaction(request.Amount, request.TransactionTimestampUtc);
        if (applyResult.IsFailure)
        {
            return Result<TrackerUpdateResultDto>.Failure(applyResult.Error);
        }

        bool newlyCompleted = applyResult.Value;
        bool rewardQueued = false;

        if (newlyCompleted)
        {
            var assignment = await QueryFirstOrDefaultAsync(
                _dbContext.CustomerOfferAssignments,
                a => a.AssignmentId == tracker.AssignmentId,
                cancellationToken);

            if (assignment is null)
            {
                return Result<TrackerUpdateResultDto>.Failure(DomainError.NotFound(
                    "OFFER_ASSIGNMENT_NOT_FOUND",
                    $"Offer assignment (ID={tracker.AssignmentId}) could not be resolved for reward fulfillment."));
            }

            decimal rewardAmount = assignment.AssignedRewardAmount;
            string rewardType = assignment.RewardType;

            var budgetAllocation = campaign.AllocateRewardBudget(rewardAmount);
            if (budgetAllocation.IsFailure)
            {
                return Result<TrackerUpdateResultDto>.Failure(budgetAllocation.Error);
            }

            var rewardItem = new RewardFulfillmentQueue
            {
                TrackerId = tracker.TrackerId,
                CustomerId = tracker.CustomerId,
                CampaignId = tracker.CampaignId,
                RewardAmount = rewardAmount,
                RewardType = rewardType,
                Status = "PendingSettlement",
                ScheduledLoadDate = request.TransactionTimestampUtc.AddDays(1),
                UpdatedAt = request.TransactionTimestampUtc
            };

            _dbContext.AddEntity(rewardItem);
            _dbContext.UpdateEntity(campaign);
            rewardQueued = true;
        }

        stopwatch.Stop();
        txLedger.ProcessingLatencyMs = Math.Round((decimal)stopwatch.Elapsed.TotalMilliseconds, 2);

        _dbContext.AddEntity(txLedger);
        _dbContext.UpdateEntity(tracker);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<TrackerUpdateResultDto>.Success(new TrackerUpdateResultDto(
            TransactionId: txLedger.TransactionId,
            TrackerId: tracker.TrackerId,
            CustomerId: tracker.CustomerId,
            CampaignId: tracker.CampaignId,
            ResolvedBrand: pos.CleanedBrandName,
            ResolvedSector: pos.ResolvedSector,
            IsAcceptedForCampaign: true,
            IsExcludedByAntiGaming: false,
            ExclusionOrClawbackReason: null,
            TargetAmount: tracker.TargetAmount,
            CurrentQualifiedSpend: tracker.CurrentQualifiedSpend,
            RemainingAmount: tracker.RemainingAmount,
            ProgressPercentage: tracker.ProgressPercentage,
            IsGoalCompleted: tracker.IsCompleted,
            RewardQueuedOrLoaded: rewardQueued || tracker.IsCompleted,
            RewardClawedBack: false,
            ProcessingLatencyMs: txLedger.ProcessingLatencyMs));
    }

    /// <summary>
    /// Processes a reversal or refund message, deducts from the tracker, and claws back any earned reward
    /// if total qualified spend drops below the target amount.
    /// </summary>
    public async Task<Result<TrackerUpdateResultDto>> ProcessReversalOrRefundAsync(
        ReversalTransactionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        if (request is null)
        {
            return Result<TrackerUpdateResultDto>.Failure(DomainError.Validation(
                "INVALID_REVERSAL_REQUEST",
                "Reversal request cannot be null."));
        }

        var validation = _reversalValidator.Validate(request);
        if (!validation.IsValid)
        {
            return Result<TrackerUpdateResultDto>.Failure(DomainError.Validation(
                "INVALID_REVERSAL_REQUEST",
                string.Join(" ", validation.Errors.Select(e => e.ErrorMessage))));
        }

        string trimmedReversalCode = request.ReversalAuthCode.Trim();
        bool isDuplicateReversal = await QueryAnyAsync(
            _dbContext.TransactionLedgerFeeds,
            tx => tx.CustomerId == request.CustomerId
                  && tx.AuthorizationCode == trimmedReversalCode
                  && tx.IsReversalOrRefund,
            cancellationToken);

        if (isDuplicateReversal)
        {
            return Result<TrackerUpdateResultDto>.Failure(DomainError.Conflict(
                "DUPLICATE_REVERSAL_CODE",
                $"Reversal authorization code ({trimmedReversalCode}) has already been processed."));
        }

        var originalTx = await QueryFirstOrDefaultAsync(
            _dbContext.TransactionLedgerFeeds,
            tx => tx.TransactionId == request.OriginalTransactionId,
            cancellationToken);

        if (originalTx is not null)
        {
            if (originalTx.CustomerId != request.CustomerId)
            {
                return Result<TrackerUpdateResultDto>.Failure(DomainError.Validation(
                    "ORIGINAL_TRANSACTION_CUSTOMER_MISMATCH",
                    $"Original transaction (ID={request.OriginalTransactionId}) does not belong to Customer (ID={request.CustomerId})."));
            }

            if (originalTx.IsReversalOrRefund || originalTx.IsExcludedByAntiGaming)
            {
                return Result<TrackerUpdateResultDto>.Failure(DomainError.Validation(
                    "ORIGINAL_TRANSACTION_NOT_REVERSIBLE",
                    $"Original transaction (ID={request.OriginalTransactionId}) was excluded or is already a reversal and cannot be refunded against qualified spend."));
            }

            decimal priorRefunded = (await QueryToListAsync(
                    _dbContext.TransactionLedgerFeeds.Where(tx => tx.OriginalTransactionId == request.OriginalTransactionId && tx.IsReversalOrRefund),
                    cancellationToken))
                .Sum(tx => tx.Amount);

            if (priorRefunded + request.RefundAmount > originalTx.Amount)
            {
                return Result<TrackerUpdateResultDto>.Failure(DomainError.Validation(
                    "REFUND_EXCEEDS_ORIGINAL_AMOUNT",
                    $"Cumulative refund ({priorRefunded + request.RefundAmount:N2} TRY) exceeds original transaction amount ({originalTx.Amount:N2} TRY)."));
            }
        }

        var tracker = await QueryFirstOrDefaultAsync(
            _dbContext.OfferProgressTrackers,
            t => t.CustomerId == request.CustomerId && t.CampaignId == request.CampaignId,
            cancellationToken);

        if (tracker is null)
        {
            return Result<TrackerUpdateResultDto>.Failure(DomainError.NotFound(
                "ACTIVE_TRACKER_NOT_FOUND",
                $"No active spend goal tracker found for Customer (ID={request.CustomerId})."));
        }

        var refundResult = tracker.ApplyReversalOrRefund(request.RefundAmount, request.ReversalTimestampUtc);
        if (refundResult.IsFailure)
        {
            return Result<TrackerUpdateResultDto>.Failure(refundResult.Error);
        }

        bool requiresClawback = refundResult.Value;
        bool rewardClawedBack = false;
        string? clawbackReason = null;

        if (requiresClawback)
        {
            clawbackReason = $"After refund/reversal ({request.RefundAmount:N2} TRY), qualified spend ({tracker.CurrentQualifiedSpend:N2} TRY) dropped below target ({tracker.TargetAmount:N2} TRY); reward was clawed back.";

            var activeRewards = await QueryToListAsync(
                _dbContext.RewardFulfillmentQueues.Where(r => r.TrackerId == tracker.TrackerId && r.Status != "ClawedBack"),
                cancellationToken);

            if (activeRewards.Count > 0)
            {
                var campaign = await QueryFirstOrDefaultAsync(
                    _dbContext.CampaignTemplates,
                    c => c.CampaignId == tracker.CampaignId,
                    cancellationToken);

                foreach (var rewardEntry in activeRewards)
                {
                    var clawbackRes = rewardEntry.TriggerClawback(clawbackReason, request.ReversalTimestampUtc);
                    if (clawbackRes.IsSuccess)
                    {
                        _dbContext.UpdateEntity(rewardEntry);
                        campaign?.ReleaseRewardBudget(rewardEntry.RewardAmount);
                        rewardClawedBack = true;
                    }
                }

                if (campaign is not null && rewardClawedBack)
                {
                    _dbContext.UpdateEntity(campaign);
                }
            }
        }

        stopwatch.Stop();
        decimal latencyMs = Math.Round((decimal)stopwatch.Elapsed.TotalMilliseconds, 2);

        var reversalLedger = new TransactionLedgerFeed
        {
            AuthorizationCode = request.ReversalAuthCode.Trim(),
            CustomerId = request.CustomerId,
            TransactionTimestamp = request.ReversalTimestampUtc,
            Amount = request.RefundAmount,
            Currency = "TRY",
            RawMcc_DE18 = "0000",
            RawMerchantName_DE43 = "REVERSAL / REFUND TRANSACTION",
            ResolvedBrand = "REFUND",
            ResolvedSector = "Reversal / Refund",
            ClassificationLayer = 1,
            IsReversalOrRefund = true,
            OriginalTransactionId = request.OriginalTransactionId,
            IsExcludedByAntiGaming = false,
            ExclusionReason = clawbackReason,
            ProcessingLatencyMs = latencyMs
        };

        _dbContext.AddEntity(reversalLedger);
        _dbContext.UpdateEntity(tracker);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<TrackerUpdateResultDto>.Success(new TrackerUpdateResultDto(
            TransactionId: reversalLedger.TransactionId,
            TrackerId: tracker.TrackerId,
            CustomerId: tracker.CustomerId,
            CampaignId: tracker.CampaignId,
            ResolvedBrand: reversalLedger.ResolvedBrand,
            ResolvedSector: reversalLedger.ResolvedSector,
            IsAcceptedForCampaign: true,
            IsExcludedByAntiGaming: false,
            ExclusionOrClawbackReason: clawbackReason,
            TargetAmount: tracker.TargetAmount,
            CurrentQualifiedSpend: tracker.CurrentQualifiedSpend,
            RemainingAmount: tracker.RemainingAmount,
            ProgressPercentage: tracker.ProgressPercentage,
            IsGoalCompleted: tracker.IsCompleted,
            RewardQueuedOrLoaded: tracker.IsCompleted,
            RewardClawedBack: rewardClawedBack,
            ProcessingLatencyMs: latencyMs));
    }

    private async Task<Result<TrackerUpdateResultDto>> RecordExcludedTransactionAsync(
        TransactionLedgerFeed txLedger,
        OfferProgressTracker tracker,
        string exclusionReason,
        bool isAntiGamingViolation,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        if (isAntiGamingViolation)
        {
            txLedger.MarkExcludedByAntiGaming(exclusionReason);
            tracker.IncrementExcludedTransactionCount(txLedger.TransactionTimestamp);
            _dbContext.UpdateEntity(tracker);
        }
        else
        {
            txLedger.IsExcludedByAntiGaming = false;
            txLedger.ExclusionReason = exclusionReason;
        }

        stopwatch.Stop();
        txLedger.ProcessingLatencyMs = Math.Round((decimal)stopwatch.Elapsed.TotalMilliseconds, 2);

        _dbContext.AddEntity(txLedger);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<TrackerUpdateResultDto>.Success(new TrackerUpdateResultDto(
            TransactionId: txLedger.TransactionId,
            TrackerId: tracker.TrackerId,
            CustomerId: tracker.CustomerId,
            CampaignId: tracker.CampaignId,
            ResolvedBrand: txLedger.ResolvedBrand,
            ResolvedSector: txLedger.ResolvedSector,
            IsAcceptedForCampaign: false,
            IsExcludedByAntiGaming: isAntiGamingViolation,
            ExclusionOrClawbackReason: exclusionReason,
            TargetAmount: tracker.TargetAmount,
            CurrentQualifiedSpend: tracker.CurrentQualifiedSpend,
            RemainingAmount: tracker.RemainingAmount,
            ProgressPercentage: tracker.ProgressPercentage,
            IsGoalCompleted: tracker.IsCompleted,
            RewardQueuedOrLoaded: tracker.IsCompleted,
            RewardClawedBack: false,
            ProcessingLatencyMs: txLedger.ProcessingLatencyMs));
    }

    private static async Task<T?> QueryFirstOrDefaultAsync<T>(
        IQueryable<T> queryable,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return queryable.Provider is IAsyncQueryProvider
            ? await queryable.FirstOrDefaultAsync(predicate, cancellationToken)
            : queryable.FirstOrDefault(predicate);
    }

    private static async Task<bool> QueryAnyAsync<T>(
        IQueryable<T> queryable,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return queryable.Provider is IAsyncQueryProvider
            ? await queryable.AnyAsync(predicate, cancellationToken)
            : queryable.Any(predicate);
    }

    private static async Task<List<T>> QueryToListAsync<T>(
        IQueryable<T> queryable,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return queryable.Provider is IAsyncQueryProvider
            ? await queryable.ToListAsync(cancellationToken)
            : queryable.ToList();
    }
}
