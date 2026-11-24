using System.Collections.Frozen;

namespace CausalCampaign.Domain.Constants;

/// <summary>
/// Constant thresholds and excluded MCC lists used to prevent campaign abuse (Anti-Gaming).
/// </summary>
public static class AntiGamingConstants
{
    /// <summary>
    /// Time window in minutes within which consecutive split transactions at the same merchant are blocked.
    /// </summary>
    public const int SplitTransactionWindowMinutes = 30;

    /// <summary>
    /// Maximum fraction of a customer's available credit limit that can be assigned as a spend target (80%).
    /// </summary>
    public const decimal MaxAvailableCreditLimitTargetRatio = 0.80m;

    /// <summary>
    /// ISO 8583 DE-18 MCC codes excluded from campaigns (cash advance, wire transfer, FX, gold/jewelry, crypto, gambling).
    /// </summary>
    public static readonly FrozenSet<string> CashLikeAndExcludedMccCodes = new[]
    {
        "4829", // Wire Transfer / Money Orders
        "6010", // Manual Cash Disbursements
        "6011", // Automated Cash Disbursements (ATM)
        "6012", // Financial Institutions - Merchandise and Debt Repayment
        "6051", // Quasi-Cash, Crypto, Foreign Exchange, Gold Bullion & Jewelry
        "6211", // Security Brokers / Dealers
        "6540", // Prepaid Card / Wallet Top-Up
        "7995"  // Betting, Lottery, and Casino Gambling
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Checks whether the specified MCC code is in the cash-like or excluded list.
    /// </summary>
    /// <param name="rawMcc">ISO 8583 DE-18 MCC code.</param>
    /// <returns>True if excluded; otherwise false.</returns>
    public static bool IsExcludedMcc(string? rawMcc)
    {
        if (string.IsNullOrWhiteSpace(rawMcc))
        {
            return true;
        }

        string trimmed = rawMcc.Trim();
        if (trimmed.Length != 4 || !trimmed.All(char.IsAsciiDigit))
        {
            return true;
        }

        return CashLikeAndExcludedMccCodes.Contains(trimmed);
    }
}
