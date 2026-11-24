using CausalCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CausalCampaign.Infrastructure.Persistence.CompiledQueries;

/// <summary>
/// Pre-compiled EF Core queries (EF.CompileAsyncQuery) that achieve sub-500 ms real-time tracker
/// update performance (KPI-5) without Dapper.
/// </summary>
public static class CausalCampaignCompiledQueries
{
    /// <summary>
    /// Retrieves a single merchant cache entry from CasualCampaign_MERCHANT_MASTER_CACHE by raw merchant key.
    /// </summary>
    public static readonly Func<CausalCampaignDbContext, string, CancellationToken, Task<MerchantMasterCache?>>
        GetMerchantCacheByKeyAsync = EF.CompileAsyncQuery(
            (CausalCampaignDbContext db, string rawKey, CancellationToken ct) =>
                db.MerchantMasterCachesSet
                    .AsNoTracking()
                    .FirstOrDefault(m => m.RawMerchantKey == rawKey));

    /// <summary>
    /// Retrieves the active offer progress tracker from CasualCampaign_OFFER_PROGRESS_TRACKER by customer and campaign ID.
    /// </summary>
    public static readonly Func<CausalCampaignDbContext, int, int, CancellationToken, Task<OfferProgressTracker?>>
        GetActiveTrackerByCustomerAndCampaignAsync = EF.CompileAsyncQuery(
            (CausalCampaignDbContext db, int customerId, int campaignId, CancellationToken ct) =>
                db.OfferProgressTrackersSet
                    .FirstOrDefault(t => t.CustomerId == customerId && t.CampaignId == campaignId));

    /// <summary>
    /// Anti-Gaming Rule 2: Checks whether the customer has a qualified transaction at the same resolved brand within the split window (30 min).
    /// </summary>
    public static readonly Func<CausalCampaignDbContext, int, string, DateTime, CancellationToken, Task<bool>>
        HasRecentBrandTransactionWithinWindowAsync = EF.CompileAsyncQuery(
            (CausalCampaignDbContext db, int customerId, string resolvedBrand, DateTime windowStartUtc, CancellationToken ct) =>
                db.TransactionLedgerFeedsSet
                    .AsNoTracking()
                    .Any(tx => tx.CustomerId == customerId
                               && tx.ResolvedBrand == resolvedBrand
                               && !tx.IsReversalOrRefund
                               && !tx.IsExcludedByAntiGaming
                               && tx.TransactionTimestamp >= windowStartUtc));

    /// <summary>
    /// Retrieves a pending or loaded reward entry from CasualCampaign_REWARD_FULFILLMENT_QUEUE for Clawback processing.
    /// </summary>
    public static readonly Func<CausalCampaignDbContext, int, CancellationToken, Task<RewardFulfillmentQueue?>>
        GetPendingOrLoadedRewardByTrackerIdAsync = EF.CompileAsyncQuery(
            (CausalCampaignDbContext db, int trackerId, CancellationToken ct) =>
                db.RewardFulfillmentQueuesSet
                    .FirstOrDefault(r => r.TrackerId == trackerId && r.Status != "ClawedBack"));
}
