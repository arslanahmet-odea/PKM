using CausalCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausalCampaign.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the dbo.vw_CasualCampaign_CustomerCausalFeatures view.
/// </summary>
public sealed class CustomerCausalFeatureViewConfiguration : IEntityTypeConfiguration<CustomerCausalFeatureView>
{
    public const string ViewName = "vw_CasualCampaign_CustomerCausalFeatures";

    public void Configure(EntityTypeBuilder<CustomerCausalFeatureView> builder)
    {
        builder.ToView(ViewName, "dbo");
        builder.HasKey(x => x.CustomerId);

        builder.Property(x => x.TotalCreditLimit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.AvailableCreditLimit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.AvailableLimitRatio).HasColumnType("decimal(6,4)");
        builder.Property(x => x.MisMonthlyNetProfit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.HistoricalBaselineSpend).HasColumnType("decimal(18,2)");
        builder.Property(x => x.RealBaselineSpendIndex).HasColumnType("decimal(18,2)");
        builder.Property(x => x.RecentNominalSpend).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PortfolioRealSpendQuantile).HasColumnType("decimal(6,4)");
    }
}
