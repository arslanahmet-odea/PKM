using CausalCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausalCampaign.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the dbo.CasualCampaign_TRANSACTION_LEDGER_FEED table.
/// </summary>
public sealed class TransactionLedgerFeedConfiguration : IEntityTypeConfiguration<TransactionLedgerFeed>
{
    public const string TableName = "CasualCampaign_TRANSACTION_LEDGER_FEED";

    public void Configure(EntityTypeBuilder<TransactionLedgerFeed> builder)
    {
        builder.ToTable(TableName, "dbo");
        builder.HasKey(x => x.TransactionId);

        builder.HasIndex(x => new { x.CustomerId, x.TransactionTimestamp });
        builder.HasIndex(x => new { x.CustomerId, x.AuthorizationCode, x.IsReversalOrRefund });
        builder.HasIndex(x => new { x.CustomerId, x.ResolvedBrand, x.TransactionTimestamp });

        builder.Property(x => x.AuthorizationCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.RawMcc_DE18).HasMaxLength(10).IsRequired();
        builder.Property(x => x.RawMerchantName_DE43).HasMaxLength(250).IsRequired();
        builder.Property(x => x.ResolvedBrand).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ResolvedSector).HasMaxLength(80).IsRequired();
        builder.Property(x => x.ExclusionReason).HasMaxLength(250);
        builder.Property(x => x.ProcessingLatencyMs).HasColumnType("decimal(10,2)").IsRequired();
    }
}
