using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CausalCampaign.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext managing the 8 CasualCampaign_ prefixed core tables and the
/// vw_CasualCampaign_CustomerCausalFeatures view in the target SQL Server database.
/// </summary>
public class CausalCampaignDbContext : DbContext, ICausalCampaignDbContext
{
    public CausalCampaignDbContext(DbContextOptions<CausalCampaignDbContext> options)
        : base(options)
    {
    }

    public DbSet<CampaignTemplate> CampaignTemplatesSet => Set<CampaignTemplate>();
    public DbSet<CustomerProfileMis> CustomerProfilesSet => Set<CustomerProfileMis>();
    public DbSet<InflationIndexMonthly> InflationIndicesSet => Set<InflationIndexMonthly>();
    public DbSet<MerchantMasterCache> MerchantMasterCachesSet => Set<MerchantMasterCache>();
    public DbSet<CustomerOfferAssignment> CustomerOfferAssignmentsSet => Set<CustomerOfferAssignment>();
    public DbSet<TransactionLedgerFeed> TransactionLedgerFeedsSet => Set<TransactionLedgerFeed>();
    public DbSet<OfferProgressTracker> OfferProgressTrackersSet => Set<OfferProgressTracker>();
    public DbSet<RewardFulfillmentQueue> RewardFulfillmentQueuesSet => Set<RewardFulfillmentQueue>();
    public DbSet<CustomerCausalFeatureView> CustomerCausalFeaturesSet => Set<CustomerCausalFeatureView>();

    IQueryable<CampaignTemplate> ICausalCampaignDbContext.CampaignTemplates => CampaignTemplatesSet;
    IQueryable<CustomerProfileMis> ICausalCampaignDbContext.CustomerProfiles => CustomerProfilesSet;
    IQueryable<InflationIndexMonthly> ICausalCampaignDbContext.InflationIndices => InflationIndicesSet;
    IQueryable<MerchantMasterCache> ICausalCampaignDbContext.MerchantMasterCaches => MerchantMasterCachesSet;
    IQueryable<CustomerOfferAssignment> ICausalCampaignDbContext.CustomerOfferAssignments => CustomerOfferAssignmentsSet;
    IQueryable<TransactionLedgerFeed> ICausalCampaignDbContext.TransactionLedgerFeeds => TransactionLedgerFeedsSet;
    IQueryable<OfferProgressTracker> ICausalCampaignDbContext.OfferProgressTrackers => OfferProgressTrackersSet;
    IQueryable<RewardFulfillmentQueue> ICausalCampaignDbContext.RewardFulfillmentQueues => RewardFulfillmentQueuesSet;
    IQueryable<CustomerCausalFeatureView> ICausalCampaignDbContext.CustomerCausalFeatures => CustomerCausalFeaturesSet.AsNoTracking();

    public void AddEntity<TEntity>(TEntity entity) where TEntity : class
    {
        Set<TEntity>().Add(entity);
    }

    public void UpdateEntity<TEntity>(TEntity entity) where TEntity : class
    {
        Set<TEntity>().Update(entity);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CausalCampaignDbContext).Assembly);
    }
}
