using CausalCampaign.Domain.Enums;
using CausalCampaign.PosEnrichment.Layers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.PosEnrichmentTests;

[TestClass]
public class PaymentFacilitatorRegexLayerTests
{
    private readonly PaymentFacilitatorRegexLayer _layer = new();

    [TestMethod]
    [DataRow("IYZICO*TRENDYOL.COM ISTANBUL", "5399", "IYZICO", "TRENDYOL", "E-Commerce & Marketplace")]
    [DataRow("PARAM*GETIR PERAKENDE LOJISTIK", "7399", "PARAM", "GETIR", "Grocery & Supermarket")]
    [DataRow("PAYTR*HEPSIBURADA D-MARKET", "5399", "PAYTR", "HEPSIBURADA", "E-Commerce & Marketplace")]
    [DataRow("SIPAY*ZARA TURKIYE GIYIM", "5651", "SIPAY", "ZARA", "Apparel & Fashion")]
    [DataRow("MOKA*YEMEKSEPETI BANABI", "5812", "MOKA", "YEMEKSEPETI", "Restaurant & Cafe")]
    [DataRow("ODEAL*STARBUCKS COFFEE KADIKOY", "5814", "ODEAL", "STARBUCKS", "Restaurant & Cafe")]
    public void TryClassify_WhenPaymentFacilitatorWrapsKnownBrand_ExtractsProviderAndBrand(
        string rawText,
        string mcc,
        string expectedProvider,
        string expectedBrand,
        string expectedSector)
    {
        // Act
        var match = _layer.TryClassify(rawText, mcc);

        // Assert
        Assert.IsNotNull(match);
        Assert.AreEqual(expectedProvider, match.SubMerchantProvider);
        Assert.AreEqual(expectedBrand, match.CleanedBrandName);
        Assert.AreEqual(expectedSector, match.ResolvedSector);
        Assert.AreEqual(PosClassificationLayer.PaymentFacilitatorRegex, match.Layer);
        Assert.IsFalse(match.IsCashLikeOrExcludedMcc);
    }

    [TestMethod]
    public void ParsePaymentFacilitator_WhenLongTailMerchantBehindPayTr_StripsPrefixAndLocation()
    {
        // Act
        var (provider, cleanedSubText) = _layer.ParsePaymentFacilitator("PAYTR*BEREKET UNLU MAMULLERI GIDA ISTANBUL");

        // Assert
        Assert.AreEqual("PAYTR", provider);
        Assert.AreEqual("BEREKET UNLU MAMULLERI GIDA", cleanedSubText);
    }
}
