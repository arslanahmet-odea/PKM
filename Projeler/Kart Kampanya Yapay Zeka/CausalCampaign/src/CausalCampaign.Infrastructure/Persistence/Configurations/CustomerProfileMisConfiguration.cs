using CausalCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausalCampaign.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the dbo.CasualCampaign_CUSTOMER_PROFILE_MIS table.
/// </summary>
public sealed class CustomerProfileMisConfiguration : IEntityTypeConfiguration<CustomerProfileMis>
{
    public const string TableName = "CasualCampaign_CUSTOMER_PROFILE_MIS";

    public void Configure(EntityTypeBuilder<CustomerProfileMis> builder)
    {
        builder.ToTable(TableName, "dbo");
        builder.HasKey(x => x.CustomerId);

        builder.Property(x => x.CustomerExternalNo).HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.CustomerExternalNo).IsUnique();

        builder.Property(x => x.FullName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CardNumberMasked).HasMaxLength(25).IsRequired();
        builder.Property(x => x.PrestigeSegment).HasMaxLength(50).IsRequired();

        builder.Property(x => x.TotalCreditLimit).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.AvailableCreditLimit).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.MisMonthlyNetProfit).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.HistoricalBaselineSpend).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.RealBaselineSpendIndex).HasColumnType("decimal(18,2)").IsRequired();

        builder.Property(x => x.SpendElasticityPersona).HasMaxLength(40).IsRequired();
        builder.Property(x => x.PreferredRewardType).HasMaxLength(30).IsRequired();
    }
}
