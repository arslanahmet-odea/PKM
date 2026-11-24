using CausalCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausalCampaign.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the dbo.CasualCampaign_OFFER_PROGRESS_TRACKER table and RowVersion concurrency token.
/// </summary>
public sealed class OfferProgressTrackerConfiguration : IEntityTypeConfiguration<OfferProgressTracker>
{
    public const string TableName = "CasualCampaign_OFFER_PROGRESS_TRACKER";

    public void Configure(EntityTypeBuilder<OfferProgressTracker> builder)
    {
        builder.ToTable(TableName, "dbo");
        builder.HasKey(x => x.TrackerId);

        builder.HasIndex(x => x.AssignmentId).IsUnique();
        builder.HasIndex(x => new { x.CustomerId, x.CampaignId });

        builder.Property(x => x.TargetAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.CurrentQualifiedSpend).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.RemainingAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ProgressPercentage).HasColumnType("decimal(6,2)").IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
