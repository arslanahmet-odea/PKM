namespace CausalCampaign.Domain.Entities;

/// <summary>
/// Entity representing the TRANSACTION_LEDGER_FEED table for ISO 8583 authorization/settlement and Anti-Gaming audit logs.
/// </summary>
public class TransactionLedgerFeed
{
    public long TransactionId { get; set; }
    public string AuthorizationCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public DateTime TransactionTimestamp { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string RawMcc_DE18 { get; set; } = string.Empty;
    public string RawMerchantName_DE43 { get; set; } = string.Empty;
    public string ResolvedBrand { get; set; } = string.Empty;
    public string ResolvedSector { get; set; } = string.Empty;
    public byte ClassificationLayer { get; set; } = 1;
    public bool IsReversalOrRefund { get; set; }
    public long? OriginalTransactionId { get; set; }
    public bool IsExcludedByAntiGaming { get; set; }
    public string? ExclusionReason { get; set; }
    public decimal ProcessingLatencyMs { get; set; }

    /// <summary>
    /// Marks the transaction as excluded due to an Anti-Gaming (abuse prevention) rule.
    /// </summary>
    public void MarkExcludedByAntiGaming(string reason)
    {
        IsExcludedByAntiGaming = true;
        ExclusionReason = reason;
    }
}
