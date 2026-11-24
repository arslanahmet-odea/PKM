using System.Text.RegularExpressions;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.Enums;

namespace CausalCampaign.PosEnrichment.Layers;

/// <summary>
/// Layer 2: Parses payment facilitator prefixes (IYZICO*, PARAM*, PAYTR*, SIPAY*, MOKA*, ODEAL*, CRAFTGATE*, PAYCELL*, PAPARA*)
/// and extracts the sub-merchant brand using compile-time [GeneratedRegex].
/// </summary>
public sealed partial class PaymentFacilitatorRegexLayer : IPosClassificationLayerHandler
{
    /// <summary>
    /// Extracts the payment facilitator provider name and cleaned sub-merchant text.
    /// </summary>
    public (string? Provider, string CleanedSubMerchantText) ParsePaymentFacilitator(string normalizedMerchantText)
    {
        if (string.IsNullOrWhiteSpace(normalizedMerchantText))
        {
            return (null, string.Empty);
        }

        var match = PaymentFacilitatorPrefixRegex().Match(normalizedMerchantText);
        if (!match.Success)
        {
            return (null, normalizedMerchantText);
        }

        string provider = match.Groups["provider"].Value.ToUpperInvariant();
        string rawSubMerchant = match.Groups["submerchant"].Value.Trim();

        string cleanedSubMerchant = LocationAndTerminalSuffixRegex().Replace(rawSubMerchant, string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(cleanedSubMerchant))
        {
            cleanedSubMerchant = rawSubMerchant;
        }

        return (provider, cleanedSubMerchant);
    }

    /// <summary>
    /// Resolves known brands behind a payment facilitator; returns null to delegate long-tail sub-merchants to Layer 3.
    /// </summary>
    public LayerClassificationMatch? TryClassify(string normalizedMerchantText, string rawMcc)
    {
        var (provider, subMerchantText) = ParsePaymentFacilitator(normalizedMerchantText);
        if (provider is null)
        {
            return null;
        }

        bool isExcludedMcc = AntiGamingConstants.IsExcludedMcc(rawMcc);

        var knownBrand = DeterministicBrandDictionaryLayer.TryMatchKnownBrand(subMerchantText);
        if (knownBrand is not null)
        {
            bool isExcluded = isExcludedMcc || knownBrand.Value.Sector == "Cash-Like / Excluded";
            return new LayerClassificationMatch(
                CleanedBrandName: knownBrand.Value.Brand,
                ResolvedSector: isExcluded ? "Cash-Like / Excluded" : knownBrand.Value.Sector,
                SubMerchantProvider: provider,
                Layer: PosClassificationLayer.PaymentFacilitatorRegex,
                ConfidenceScore: 0.9750m,
                IsCashLikeOrExcludedMcc: isExcluded);
        }

        return null;
    }

    [GeneratedRegex(@"^(?<provider>IYZICO|PARAM|PAYTR|SIPAY|MOKA|ODEAL|CRAFTGATE|PAYCELL|PAPARA|PAYNET)\s*[\*\-/]\s*(?<submerchant>.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PaymentFacilitatorPrefixRegex();

    [GeneratedRegex(@"\s+(ISTANBUL|ANKARA|IZMIR|ANTALYA|BURSA|KADIKOY|ATASEHIR|MASLAK|BESIKTAS|SISLI|TR|TUR|POS\d+|TERM\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LocationAndTerminalSuffixRegex();
}
