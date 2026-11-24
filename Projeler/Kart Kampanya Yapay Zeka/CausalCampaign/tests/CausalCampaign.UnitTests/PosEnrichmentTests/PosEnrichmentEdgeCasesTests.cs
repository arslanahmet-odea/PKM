using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Enums;
using CausalCampaign.PosEnrichment.Caching;
using CausalCampaign.PosEnrichment.Layers;
using CausalCampaign.PosEnrichment.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.PosEnrichmentTests;

[TestClass]
public class PosEnrichmentEdgeCasesTests
{
    private static PosEnrichmentService CreateService(int maxL1Entries = 50_000)
    {
        var cacheManager = new PosMerchantCacheManager(maxL1Entries);
        var layer1 = new DeterministicBrandDictionaryLayer();
        var layer2 = new PaymentFacilitatorRegexLayer();
        var layer3 = new LocalMlMerchantClassifierLayer(layer2);
        return new PosEnrichmentService(cacheManager, layer1, layer2, layer3);
    }

    [TestMethod]
    public async Task EnrichTransactionMerchantAsync_SameMerchantWithDifferentMccs_DoesNotPoisonCache()
    {
        var service = CreateService();

        // 1. First transaction: Grocery MCC 5411
        var groceryRes = await service.EnrichTransactionMerchantAsync(
            new PosEnrichmentRequestDto("PAYTR*ONLINE PERAKENDE SATIS", "5411"));
        Assert.IsTrue(groceryRes.IsSuccess);
        Assert.IsFalse(groceryRes.Value.IsCashLikeOrExcludedMcc);
        Assert.AreEqual("5411", groceryRes.Value.RawMcc_DE18);

        // 2. Second transaction: Same DE-43 descriptor, but Crypto/Quasi-Cash MCC 6051
        var cryptoRes = await service.EnrichTransactionMerchantAsync(
            new PosEnrichmentRequestDto("PAYTR*ONLINE PERAKENDE SATIS", "6051"));
        Assert.IsTrue(cryptoRes.IsSuccess);
        Assert.IsFalse(cryptoRes.Value.ServedFromCache); // Distinct composite cache key!
        Assert.IsTrue(cryptoRes.Value.IsCashLikeOrExcludedMcc);
        Assert.AreEqual("6051", cryptoRes.Value.RawMcc_DE18);
        Assert.AreEqual("Cash-Like / Excluded", cryptoRes.Value.ResolvedSector);

        // 3. Third transaction: Re-verify 5411 is still unpoisoned in L1 cache
        var groceryCachedRes = await service.EnrichTransactionMerchantAsync(
            new PosEnrichmentRequestDto("PAYTR*ONLINE PERAKENDE SATIS", "5411"));
        Assert.IsTrue(groceryCachedRes.IsSuccess);
        Assert.IsTrue(groceryCachedRes.Value.ServedFromCache);
        Assert.IsFalse(groceryCachedRes.Value.IsCashLikeOrExcludedMcc);
        Assert.AreEqual("5411", groceryCachedRes.Value.RawMcc_DE18);
    }

    [TestMethod]
    public async Task EnrichTransactionMerchantAsync_SlashSeparatorPaymentFacilitator_DelegatesToLayer2()
    {
        var service = CreateService();
        var result = await service.EnrichTransactionMerchantAsync(
            new PosEnrichmentRequestDto("IYZICO/MIGROS SANAL MARKET", "5411"));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(PosClassificationLayer.PaymentFacilitatorRegex, result.Value.ClassificationLayer);
        Assert.AreEqual("IYZICO", result.Value.SubMerchantProvider);
        Assert.AreEqual("MIGROS", result.Value.CleanedBrandName);
        Assert.AreEqual("Grocery & Supermarket", result.Value.ResolvedSector);
    }

    [TestMethod]
    public async Task PosMerchantCacheManager_EnforcesMaxL1CapacityBound()
    {
        var cacheManager = new PosMerchantCacheManager(maxL1Entries: 100);
        var layer1 = new DeterministicBrandDictionaryLayer();
        var layer2 = new PaymentFacilitatorRegexLayer();
        var layer3 = new LocalMlMerchantClassifierLayer(layer2);
        var service = new PosEnrichmentService(cacheManager, layer1, layer2, layer3);

        for (int i = 0; i < 150; i++)
        {
            await service.EnrichTransactionMerchantAsync(new PosEnrichmentRequestDto($"MIGROS STORE #{i}", "5411"));
        }

        Assert.IsTrue(cacheManager.L1CacheCount <= 100, $"Expected L1 cache count <= 100, got {cacheManager.L1CacheCount}");
    }

    [TestMethod]
    public void NormalizeMerchantKey_CollapsesTabsAndNewlines_AndEvaluateMacroF1HandlesEmptyAndNullMcc()
    {
        Assert.AreEqual("MIGROS TICARET ISTANBUL", PosMerchantCacheManager.NormalizeMerchantKey("  migros\t\tticaret\nistanbul  "));

        var service = CreateService();
        var emptyEval = service.EvaluateMacroF1Score(Array.Empty<(string, string, string)>());
        Assert.IsTrue(emptyEval.IsFailure);
        Assert.AreEqual("EMPTY_EVALUATION_DATASET", emptyEval.Error.Code);

        var nullMccEval = service.EvaluateMacroF1Score(new List<(string, string, string)>
        {
            ("MIGROS TICARET A.S.", null!, "Cash-Like / Excluded")
        });
        Assert.IsTrue(nullMccEval.IsSuccess);
        Assert.AreEqual(1, nullMccEval.Value.CorrectPredictionCount);
    }
}
