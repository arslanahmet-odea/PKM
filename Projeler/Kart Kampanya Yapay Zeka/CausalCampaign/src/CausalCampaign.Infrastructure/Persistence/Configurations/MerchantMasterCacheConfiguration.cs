using CausalCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausalCampaign.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the dbo.CasualCampaign_MERCHANT_MASTER_CACHE table.
/// </summary>
public sealed class MerchantMasterCacheConfiguration : IEntityTypeConfiguration<MerchantMasterCache>
{
    public const string TableName = "CasualCampaign_MERCHANT_MASTER_CACHE";

    public void Configure(EntityTypeBuilder<MerchantMasterCache> builder)
    {
        builder.ToTable(TableName, "dbo");
        builder.HasKey(x => x.RawMerchantKey);

        builder.Property(x => x.RawMerchantKey).HasMaxLength(250).IsRequired();
        builder.Property(x => x.RawMcc_DE18).HasMaxLength(10).IsRequired();
        builder.Property(x => x.CleanedBrandName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ResolvedSector).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SubMerchantProvider).HasMaxLength(60);
        builder.Property(x => x.ConfidenceScore).HasColumnType("decimal(6,4)").IsRequired();
    }
}
