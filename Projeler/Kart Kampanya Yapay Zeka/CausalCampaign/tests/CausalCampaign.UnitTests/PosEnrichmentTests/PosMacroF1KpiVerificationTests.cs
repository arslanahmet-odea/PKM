using CausalCampaign.Domain.Constants;
using CausalCampaign.PosEnrichment.Caching;
using CausalCampaign.PosEnrichment.Layers;
using CausalCampaign.PosEnrichment.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.PosEnrichmentTests;

[TestClass]
public class PosMacroF1KpiVerificationTests
{
    [TestMethod]
    public void EvaluateMacroF1Score_OnGroundTruthPosBenchmarkDataset_ExceedsNinetyTwoPercentKpi1Target()
    {
        // Arrange: 3-Layer POS Enrichment Service
        var cacheManager = new PosMerchantCacheManager();
        var layer1 = new DeterministicBrandDictionaryLayer();
        var layer2 = new PaymentFacilitatorRegexLayer();
        var layer3 = new LocalMlMerchantClassifierLayer(layer2);
        var service = new PosEnrichmentService(cacheManager, layer1, layer2, layer3);

        var groundTruthBenchmark = new List<(string RawMerchant_DE43, string RawMcc_DE18, string ExpectedSector)>
        {
            // Grocery & Supermarket
            ("MIGROS TICARET A.S. ISTANBUL TR", "5411", "Grocery & Supermarket"),
            ("CARREFOURSA HIPERMARKET ATASEHIR", "5411", "Grocery & Supermarket"),
            ("PARAM*GETIR PERAKENDE LOJISTIK", "7399", "Grocery & Supermarket"),
            ("BIM BIRLESIK MAGAZALAR KADIKOY", "5411", "Grocery & Supermarket"),
            ("BEREKET UNLU MAMULLERI VE GIDA SAN", "5462", "Grocery & Supermarket"),
            ("IYZICO*DOGAL SARKUTERI PEYNIR GIDA", "5499", "Grocery & Supermarket"),

            // Fuel
            ("SHELL & TURCAS PETROL MASLAK", "5541", "Fuel"),
            ("OPET PETROLCULUK KADIKOY", "5541", "Fuel"),
            ("PETROL OFISI A.S. ANKARA", "5541", "Fuel"),
            ("AKDENIZ AKARYAKIT ISTASYONU PETROL", "5541", "Fuel"),
            ("MARMARA PETROLCULUK LPG OTOGAZ", "5542", "Fuel"),

            // E-Commerce & Marketplace
            ("IYZICO*TRENDYOL.COM ISTANBUL", "5399", "E-Commerce & Marketplace"),
            ("PAYTR*HEPSIBURADA D-MARKET", "5399", "E-Commerce & Marketplace"),
            ("PARAM*AMAZON.COM.TR ISTANBUL", "5399", "E-Commerce & Marketplace"),
            ("IYZICO*CICEKSEPETI INTERNET HIZM", "5399", "E-Commerce & Marketplace"),
            ("DIJITAL PAZARYERI E TICARET PLATFORMU", "5399", "E-Commerce & Marketplace"),

            // Apparel & Fashion
            ("SIPAY*ZARA TURKIYE GIYIM", "5651", "Apparel & Fashion"),
            ("LC WAIKIKI MAGAZACILIK ISTANBUL", "5651", "Apparel & Fashion"),
            ("BOYNER BUYUK MAGAZACILIK", "5651", "Apparel & Fashion"),
            ("PAYTR*MODA BUTIK KONFEKSIYON TEKSTIL", "5651", "Apparel & Fashion"),
            ("DERI AYAKKABI CANTA AKSESUAR MAGAZASI", "5661", "Apparel & Fashion"),

            // Electronics
            ("TEKNOSA IC VE DIS TICARET", "5732", "Electronics"),
            ("MEDIAMARKT TURKIYE ELEKTRONIK", "5732", "Electronics"),
            ("VATAN BILGISAYAR SAN VE TIC", "5732", "Electronics"),
            ("IYZICO*GUNES BILGISAYAR DONANIM ELEKTRONIK", "5732", "Electronics"),

            // Restaurant & Cafe
            ("MOKA*YEMEKSEPETI BANABI", "5812", "Restaurant & Cafe"),
            ("STARBUCKS COFFEE MASLAK", "5814", "Restaurant & Cafe"),
            ("BURGER KING TAB GIDA", "5814", "Restaurant & Cafe"),
            ("ODEAL*LEZZET DURAGI KEBAP RESTORAN", "5812", "Restaurant & Cafe"),

            // Travel & Hospitality
            ("TURK HAVA YOLLARI A.O. ISTANBUL", "4511", "Travel & Hospitality"),
            ("PEGASUS HAVA TASIMACILIGI", "4511", "Travel & Hospitality"),
            ("GRAND RESORT SPA HOTEL TURIZM KONAKLAMA", "7011", "Travel & Hospitality"),

            // Health & Cosmetics
            ("WATSONS GUZELLIK VE BAKIM", "5977", "Health & Cosmetics"),
            ("GRATIS KISISEL BAKIM", "5977", "Health & Cosmetics"),
            ("SIFA ECZANESI MEDIKAL SAGLIK URUNLERI", "5912", "Health & Cosmetics"),

            // Cash-Like / Excluded
            ("ALTINBAS KUYUMCULUK KAPALICARSI", "6051", "Cash-Like / Excluded"),
            ("BILYONER INTERAKTIF HIZMETLER", "7995", "Cash-Like / Excluded"),
            ("KAPALICARSI SARRAFIYE ALTIN KUYUMCULUK", "6051", "Cash-Like / Excluded")
        };

        // Act
        var evalResult = service.EvaluateMacroF1Score(groundTruthBenchmark);

        // Assert
        Assert.IsTrue(evalResult.IsSuccess);
        Assert.IsTrue(
            evalResult.Value.MeetsKpi1Threshold,
            $"KPI-1 Macro F1 target ({KpiThresholds.MinPosClassificationMacroF1:P1}) was not met. Actual: {evalResult.Value.MacroF1Score:P2}");
        Assert.IsTrue(evalResult.Value.MacroF1Score >= 0.92m);
    }
}
