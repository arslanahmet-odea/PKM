using CausalCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausalCampaign.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the dbo.CasualCampaign_INFLATION_INDEX_MONTHLY table.
/// </summary>
public sealed class InflationIndexMonthlyConfiguration : IEntityTypeConfiguration<InflationIndexMonthly>
{
    public const string TableName = "CasualCampaign_INFLATION_INDEX_MONTHLY";

    public void Configure(EntityTypeBuilder<InflationIndexMonthly> builder)
    {
        builder.ToTable(TableName, "dbo");
        builder.HasKey(x => x.InflationIndexId);

        builder.Property(x => x.SectorName).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => new { x.YearMonth, x.SectorName }).IsUnique();

        builder.Property(x => x.CpiIndexValue).HasColumnType("decimal(12,4)").IsRequired();
        builder.Property(x => x.DeflatorMultiplier).HasColumnType("decimal(12,6)").IsRequired();
    }
}
