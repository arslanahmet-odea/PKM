using CausalCampaign.Domain.Entities;

namespace CausalCampaign.Domain.Interfaces;

/// <summary>
/// Port interface enabling Clean Architecture Domain/Application layers to access the EF Core 10.0 database context.
/// </summary>
public interface ICausalCampaignDbContext
{
    IQueryable<CampaignTemplate> CampaignTemplates { get; }
    IQueryable<CustomerProfileMis> CustomerProfiles { get; }
    IQueryable<InflationIndexMonthly> InflationIndices { get; }
    IQueryable<MerchantMasterCache> MerchantMasterCaches { get; }
    IQueryable<CustomerOfferAssignment> CustomerOfferAssignments { get; }
    IQueryable<TransactionLedgerFeed> TransactionLedgerFeeds { get; }
    IQueryable<OfferProgressTracker> OfferProgressTrackers { get; }
    IQueryable<RewardFulfillmentQueue> RewardFulfillmentQueues { get; }
    IQueryable<CustomerCausalFeatureView> CustomerCausalFeatures { get; }

    void AddEntity<TEntity>(TEntity entity) where TEntity : class;
    void UpdateEntity<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
