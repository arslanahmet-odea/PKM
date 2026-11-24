using CausalCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausalCampaign.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the dbo.CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT table.
/// </summary>
public sealed class CustomerOfferAssignmentConfiguration : IEntityTypeConfiguration<CustomerOfferAssignment>
{
    public const string TableName = "CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT";

    public void Configure(EntityTypeBuilder<CustomerOfferAssignment> builder)
    {
        builder.ToTable(TableName, "dbo");
        builder.HasKey(x => x.AssignmentId);

        builder.HasIndex(x => new { x.CustomerId, x.CampaignId });

        builder.Property(x => x.MicroBucketName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ExperimentGroup).HasMaxLength(30).IsRequired();

        builder.Property(x => x.ExpectedOrganicSpend).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.PredictedUpliftSpend).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.PropensityScore).HasColumnType("decimal(6,4)").IsRequired();
        builder.Property(x => x.ReactivationProbability).HasColumnType("decimal(6,4)").IsRequired();
        builder.Property(x => x.AssignedTargetAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.AssignedRewardAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.RewardType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ExpectedNetIncrementalProfit).HasColumnType("decimal(18,2)").IsRequired();

        builder.Property(x => x.XaiReasonText).HasMaxLength(500).IsRequired();
        builder.Property(x => x.XaiTopFeaturesJson).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.AlternativeOffersJson).HasMaxLength(2000).IsRequired();
    }
}
