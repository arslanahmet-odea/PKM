using CausalCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausalCampaign.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the dbo.CasualCampaign_REWARD_FULFILLMENT_QUEUE table.
/// </summary>
public sealed class RewardFulfillmentQueueConfiguration : IEntityTypeConfiguration<RewardFulfillmentQueue>
{
    public const string TableName = "CasualCampaign_REWARD_FULFILLMENT_QUEUE";

    public void Configure(EntityTypeBuilder<RewardFulfillmentQueue> builder)
    {
        builder.ToTable(TableName, "dbo");
        builder.HasKey(x => x.FulfillmentId);

        builder.HasIndex(x => new { x.TrackerId, x.Status });

        builder.Property(x => x.RewardAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.RewardType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ClawbackReason).HasMaxLength(300);
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
    }
}
