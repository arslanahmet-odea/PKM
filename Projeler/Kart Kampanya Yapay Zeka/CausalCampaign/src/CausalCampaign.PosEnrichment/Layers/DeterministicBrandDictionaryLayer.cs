using System.Collections.Frozen;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.Enums;

namespace CausalCampaign.PosEnrichment.Layers;

/// <summary>
/// Layer 1: Deterministically classifies national chains, major known brands, and cash-like/excluded MCCs
/// in under 0.1 ms using FrozenDictionary.
/// </summary>
public sealed class DeterministicBrandDictionaryLayer : IPosClassificationLayerHandler
{
    private static readonly FrozenDictionary<string, (string Brand, string Sector)> BrandRules =
        new Dictionary<string, (string Brand, string Sector)>(StringComparer.OrdinalIgnoreCase)
        {
            // Grocery & Supermarket
            ["MIGROS"] = ("MIGROS", "Grocery & Supermarket"),
            ["MİGROS"] = ("MIGROS", "Grocery & Supermarket"),
            ["MACROCENTER"] = ("MACROCENTER", "Grocery & Supermarket"),
            ["CARREFOUR"] = ("CARREFOURSA", "Grocery & Supermarket"),
            ["BIM BIRLESIK"] = ("BIM", "Grocery & Supermarket"),
            ["BİM BİRLEŞİK"] = ("BIM", "Grocery & Supermarket"),
            ["A101 YENI"] = ("A101", "Grocery & Supermarket"),
            ["SOK MARKET"] = ("SOK MARKET", "Grocery & Supermarket"),
            ["ŞOK MARKET"] = ("SOK MARKET", "Grocery & Supermarket"),
            ["FILE MARKET"] = ("FILE MARKET", "Grocery & Supermarket"),
            ["METRO GROSS"] = ("METRO MARKET", "Grocery & Supermarket"),
            ["GETIR"] = ("GETIR", "Grocery & Supermarket"),
            ["GETİR"] = ("GETIR", "Grocery & Supermarket"),

            // Fuel
            ["SHELL"] = ("SHELL", "Fuel"),
            ["OPET"] = ("OPET", "Fuel"),
            ["PETROL OFISI"] = ("PETROL OFISI", "Fuel"),
            ["PETROL OFİSİ"] = ("PETROL OFISI", "Fuel"),
            ["BP PETROL"] = ("BP", "Fuel"),
            ["TOTAL ENERGIES"] = ("TOTALENERGIES", "Fuel"),
            ["AYTEMIZ"] = ("AYTEMIZ", "Fuel"),

            // E-Commerce & Marketplace
            ["TRENDYOL"] = ("TRENDYOL", "E-Commerce & Marketplace"),
            ["DSM GRUP"] = ("TRENDYOL", "E-Commerce & Marketplace"),
            ["HEPSIBURADA"] = ("HEPSIBURADA", "E-Commerce & Marketplace"),
            ["D-MARKET"] = ("HEPSIBURADA", "E-Commerce & Marketplace"),
            ["AMAZON"] = ("AMAZON TR", "E-Commerce & Marketplace"),
            ["N11.COM"] = ("N11", "E-Commerce & Marketplace"),
            ["DOGAN ONLINE"] = ("HEPSIBURADA", "E-Commerce & Marketplace"),
            ["CICEKSEPETI"] = ("CICEKSEPETI", "E-Commerce & Marketplace"),

            // Apparel & Fashion
            ["LC WAIKIKI"] = ("LC WAIKIKI", "Apparel & Fashion"),
            ["LCWAIKIKI"] = ("LC WAIKIKI", "Apparel & Fashion"),
            ["DEFACTO"] = ("DEFACTO", "Apparel & Fashion"),
            ["MAVI GIYIM"] = ("MAVI", "Apparel & Fashion"),
            ["BOYNER"] = ("BOYNER", "Apparel & Fashion"),
            ["ZARA"] = ("ZARA", "Apparel & Fashion"),
            ["MANGO"] = ("MANGO", "Apparel & Fashion"),
            ["KOTON"] = ("KOTON", "Apparel & Fashion"),
            ["VAKKO"] = ("VAKKO", "Apparel & Fashion"),
            ["BEYMEN"] = ("BEYMEN", "Apparel & Fashion"),
            ["DECATHLON"] = ("DECATHLON", "Apparel & Fashion"),

            // Electronics
            ["TEKNOSA"] = ("TEKNOSA", "Electronics"),
            ["MEDIAMARKT"] = ("MEDIAMARKT", "Electronics"),
            ["MEDIA MARKT"] = ("MEDIAMARKT", "Electronics"),
            ["VATAN BILGISAYAR"] = ("VATAN BILGISAYAR", "Electronics"),
            ["APPLE STORE"] = ("APPLE", "Electronics"),
            ["SAMSUNG"] = ("SAMSUNG", "Electronics"),
            ["ARÇELİK"] = ("ARCELIK", "Electronics"),
            ["ARCELIK"] = ("ARCELIK", "Electronics"),
            ["VESTEL"] = ("VESTEL", "Electronics"),

            // Restaurant & Cafe
            ["STARBUCKS"] = ("STARBUCKS", "Restaurant & Cafe"),
            ["KAHVE DUNYASI"] = ("KAHVE DUNYASI", "Restaurant & Cafe"),
            ["YEMEKSEPETI"] = ("YEMEKSEPETI", "Restaurant & Cafe"),
            ["BURGER KING"] = ("BURGER KING", "Restaurant & Cafe"),
            ["MCDONALDS"] = ("MCDONALD'S", "Restaurant & Cafe"),
            ["DOMINOS"] = ("DOMINO'S PIZZA", "Restaurant & Cafe"),
            ["TAVUK DUNYASI"] = ("TAVUK DUNYASI", "Restaurant & Cafe"),

            // Travel & Hospitality
            ["TURK HAVA YOLLARI"] = ("THY", "Travel & Hospitality"),
            ["THY "] = ("THY", "Travel & Hospitality"),
            ["PEGASUS"] = ("PEGASUS", "Travel & Hospitality"),
            ["ANADOLUJET"] = ("AJET", "Travel & Hospitality"),
            ["AJET"] = ("AJET", "Travel & Hospitality"),
            ["ETS TUR"] = ("ETSTUR", "Travel & Hospitality"),
            ["JOLLY TUR"] = ("JOLLY TUR", "Travel & Hospitality"),
            ["HILTON"] = ("HILTON", "Travel & Hospitality"),

            // Health & Cosmetics
            ["WATSONS"] = ("WATSONS", "Health & Cosmetics"),
            ["GRATIS"] = ("GRATIS", "Health & Cosmetics"),
            ["SEPHORA"] = ("SEPHORA", "Health & Cosmetics"),
            ["ROSSMANN"] = ("ROSSMANN", "Health & Cosmetics"),
            ["ACIBADEM"] = ("ACIBADEM HEALTH", "Health & Cosmetics"),
            ["MEMORIAL"] = ("MEMORIAL HEALTH", "Health & Cosmetics"),

            // Cash-Like / Excluded
            ["ALTINBAS"] = ("ALTINBAS", "Cash-Like / Excluded"),
            ["ATASAY"] = ("ATASAY JEWELRY", "Cash-Like / Excluded"),
            ["KUYUMCU"] = ("JEWELRY", "Cash-Like / Excluded"),
            ["DOVIZ"] = ("FX EXCHANGE", "Cash-Like / Excluded"),
            ["BILYONER"] = ("BILYONER", "Cash-Like / Excluded"),
            ["NESINE"] = ("NESINE", "Cash-Like / Excluded"),
            ["MISLI"] = ("MISLI", "Cash-Like / Excluded"),
            ["BINANCE"] = ("BINANCE CRYPTO", "Cash-Like / Excluded"),
            ["BTCTURK"] = ("BTCTURK", "Cash-Like / Excluded")
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenSet<string> PaymentFacilitatorPrefixes = new[]
    {
        "IYZICO", "PARAM", "PAYTR", "SIPAY", "MOKA", "ODEAL", "CRAFTGATE", "PAYCELL", "PAPARA", "PAYNET"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Classifies direct national chains or excluded MCC transactions that do not start with a payment facilitator prefix.
    /// </summary>
    public LayerClassificationMatch? TryClassify(string normalizedMerchantText, string rawMcc)
    {
        bool isExcludedMcc = AntiGamingConstants.IsExcludedMcc(rawMcc);

        // If the text starts with a payment facilitator prefix (e.g., IYZICO*..., IYZICO/..., IYZICO-...), delegate to Layer 2
        foreach (string pfPrefix in PaymentFacilitatorPrefixes)
        {
            if (normalizedMerchantText.StartsWith(pfPrefix + "*", StringComparison.OrdinalIgnoreCase) ||
                normalizedMerchantText.StartsWith(pfPrefix + " *", StringComparison.OrdinalIgnoreCase) ||
                normalizedMerchantText.StartsWith(pfPrefix + "-", StringComparison.OrdinalIgnoreCase) ||
                normalizedMerchantText.StartsWith(pfPrefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
        }

        var matchedRule = TryMatchKnownBrand(normalizedMerchantText);
        if (matchedRule is not null)
        {
            bool isExcluded = isExcludedMcc || matchedRule.Value.Sector == "Cash-Like / Excluded";
            return new LayerClassificationMatch(
                CleanedBrandName: matchedRule.Value.Brand,
                ResolvedSector: isExcluded ? "Cash-Like / Excluded" : matchedRule.Value.Sector,
                SubMerchantProvider: null,
                Layer: PosClassificationLayer.Dictionary,
                ConfidenceScore: 0.9950m,
                IsCashLikeOrExcludedMcc: isExcluded);
        }

        if (isExcludedMcc)
        {
            return new LayerClassificationMatch(
                CleanedBrandName: ExtractFallbackBrandTitle(normalizedMerchantText),
                ResolvedSector: "Cash-Like / Excluded",
                SubMerchantProvider: null,
                Layer: PosClassificationLayer.Dictionary,
                ConfidenceScore: 0.9900m,
                IsCashLikeOrExcludedMcc: true);
        }

        return null;
    }

    /// <summary>
    /// Searches the specified text for a known national brand keyword.
    /// </summary>
    public static (string Brand, string Sector)? TryMatchKnownBrand(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        foreach (var kvp in BrandRules)
        {
            if (text.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        return null;
    }

    internal static string ExtractFallbackBrandTitle(string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return "UNKNOWN MERCHANT";
        }

        string[] tokens = normalizedText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length >= 2 ? $"{tokens[0]} {tokens[1]}" : tokens[0];
    }
}
