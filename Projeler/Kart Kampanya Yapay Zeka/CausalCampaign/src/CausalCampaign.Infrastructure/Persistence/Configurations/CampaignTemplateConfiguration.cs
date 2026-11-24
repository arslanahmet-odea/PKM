using CausalCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausalCampaign.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the dbo.CasualCampaign_CAMPAIGN_TEMPLATE table.
/// </summary>
public sealed class CampaignTemplateConfiguration : IEntityTypeConfiguration<CampaignTemplate>
{
    public const string TableName = "CasualCampaign_CAMPAIGN_TEMPLATE";

    public void Configure(EntityTypeBuilder<CampaignTemplate> builder)
    {
        builder.ToTable(TableName, "dbo");
        builder.HasKey(x => x.CampaignId);

        builder.Property(x => x.CampaignCode).HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.CampaignCode).IsUnique();

        builder.Property(x => x.CampaignName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CampaignType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.TargetSector).HasMaxLength(80).IsRequired();
        builder.Property(x => x.DefaultRewardType).HasMaxLength(30).IsRequired();

        builder.Property(x => x.TotalBudget).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ConsumedBudget).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.MaxRewardPerCustomer).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.MinMisProfitThreshold).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.InterchangeMarginRate).HasColumnType("decimal(9,4)").IsRequired();

        builder.Property(x => x.KillSwitchReason).HasMaxLength(300);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.Ignore(x => x.RemainingBudget);
    }
}
