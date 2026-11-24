using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.Enums;
using CausalCampaign.Domain.Interfaces;
using CausalCampaign.PosEnrichment.Caching;
using CausalCampaign.PosEnrichment.Configuration;
using CausalCampaign.PosEnrichment.Layers;
using CausalCampaign.PosEnrichment.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CausalCampaign.UnitTests.PosEnrichmentTests;

[TestClass]
public class PosEnrichmentServiceMoqTests
{
    private static PosEnrichmentService CreateService(ICausalCampaignDbContext? dbContext = null, bool persistToDb = false)
    {
        var cacheManager = new PosMerchantCacheManager();
        var layer1 = new DeterministicBrandDictionaryLayer();
        var layer2 = new PaymentFacilitatorRegexLayer();
        var layer3 = new LocalMlMerchantClassifierLayer(layer2);
        var options = Options.Create(new PosEnrichmentOptions
        {
            PersistNewResolutionsToDatabaseCache = persistToDb
        });

        return new PosEnrichmentService(cacheManager, layer1, layer2, layer3, options, dbContext);
    }

    [TestMethod]
    public async Task EnrichTransactionMerchantAsync_SecondCallForSameMerchant_IsServedFromL1CacheInSubMillisecond()
    {
        // Arrange
        var mockDb = new Mock<ICausalCampaignDbContext>(MockBehavior.Loose);
        var service = CreateService(mockDb.Object, persistToDb: false);
        var request = new PosEnrichmentRequestDto("IYZICO*TRENDYOL.COM ISTANBUL", "5399");

        // Act 1: First resolution (Layer 2)
        var firstResult = await service.EnrichTransactionMerchantAsync(request);
        // Act 2: Second resolution (L1 Cache)
        var secondResult = await service.EnrichTransactionMerchantAsync(request);

        // Assert
        Assert.IsTrue(firstResult.IsSuccess);
        Assert.IsFalse(firstResult.Value.ServedFromCache);
        Assert.AreEqual("TRENDYOL", firstResult.Value.CleanedBrandName);
        Assert.AreEqual(PosClassificationLayer.PaymentFacilitatorRegex, firstResult.Value.ClassificationLayer);

        Assert.IsTrue(secondResult.IsSuccess);
        Assert.IsTrue(secondResult.Value.ServedFromCache);
        Assert.IsTrue(secondResult.Value.ProcessingTimeMs < 5.0, $"L1 Cache response time is too high: {secondResult.Value.ProcessingTimeMs} ms");

        // When database persistence is disabled, AddEntity must never be called
        mockDb.Verify(db => db.AddEntity(It.IsAny<MerchantMasterCache>()), Times.Never);
    }

    [TestMethod]
    public async Task EnrichTransactionMerchantAsync_WhenLongTailMerchantNotInDictionary_UsesLayer3MachineLearning()
    {
        // Arrange
        var service = CreateService();
        var request = new PosEnrichmentRequestDto("PAYTR*ANADOLU ET GALERISI KASAP SARKUTERI", "5422");

        // Act
        var result = await service.EnrichTransactionMerchantAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(PosClassificationLayer.MachineLearning, result.Value.ClassificationLayer);
        Assert.AreEqual("Grocery & Supermarket", result.Value.ResolvedSector);
        Assert.AreEqual("PAYTR", result.Value.SubMerchantProvider);
        Assert.IsFalse(result.Value.IsCashLikeOrExcludedMcc);
    }

    [TestMethod]
    public async Task EnrichTransactionMerchantAsync_WhenCashLikeMcc6051_FlagsAsExcludedForAntiGaming()
    {
        // Arrange
        var service = CreateService();
        var request = new PosEnrichmentRequestDto("KAPALICARSI ALTIN VE DOVIZ TICARET", "6051");

        // Act
        var result = await service.EnrichTransactionMerchantAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Value.IsCashLikeOrExcludedMcc);
        Assert.AreEqual("Cash-Like / Excluded", result.Value.ResolvedSector);
    }
}
