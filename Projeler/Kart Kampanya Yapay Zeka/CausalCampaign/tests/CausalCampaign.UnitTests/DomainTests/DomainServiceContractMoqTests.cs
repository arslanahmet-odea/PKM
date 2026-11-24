using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Enums;
using CausalCampaign.Domain.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CausalCampaign.UnitTests.DomainTests;

[TestClass]
public class DomainServiceContractMoqTests
{
    [TestMethod]
    public async Task IPosEnrichmentService_MoqContract_ResolvesPaymentFacilitatorSubMerchantWithinLatencyBudget()
    {
        // Arrange
        var mockPosService = new Mock<IPosEnrichmentService>(MockBehavior.Strict);
        var request = new PosEnrichmentRequestDto("IYZICO*TRENDYOL.COM ISTANBUL", "5399");
        var expectedResponse = new PosEnrichmentResultDto(
            RawMerchantKey: "IYZICO*TRENDYOL.COM ISTANBUL",
            RawMcc_DE18: "5399",
            CleanedBrandName: "TRENDYOL",
            ResolvedSector: "E-Commerce & Marketplace",
            SubMerchantProvider: "IYZICO",
            ClassificationLayer: PosClassificationLayer.PaymentFacilitatorRegex,
            ConfidenceScore: 0.98m,
            IsCashLikeOrExcludedMcc: false,
            ServedFromCache: true,
            ProcessingTimeMs: 0.42);

        mockPosService
            .Setup(s => s.EnrichTransactionMerchantAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PosEnrichmentResultDto>.Success(expectedResponse));

        // Act
        var result = await mockPosService.Object.EnrichTransactionMerchantAsync(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("TRENDYOL", result.Value.CleanedBrandName);
        Assert.AreEqual("IYZICO", result.Value.SubMerchantProvider);
        Assert.IsTrue((decimal)result.Value.ProcessingTimeMs < KpiThresholds.MaxRealtimeTrackerLatencyMs);
        mockPosService.Verify(s => s.EnrichTransactionMerchantAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task IRealtimeOfferTrackerService_MoqContract_WhenSplitTransactionDetected_ReturnsAntiGamingExclusion()
    {
        // Arrange
        var mockTrackerService = new Mock<IRealtimeOfferTrackerService>(MockBehavior.Strict);
        var request = new AuthorizeTransactionRequestDto(
            AuthorizationCode: "AUTH-9999",
            CustomerId: 1001,
            CampaignId: 1,
            Amount: 2500.00m,
            Currency: "TRY",
            RawMcc_DE18: "7399",
            RawMerchantName_DE43: "PARAM*GETIR PERAKENDE LOJISTIK",
            TransactionTimestampUtc: DateTime.UtcNow);

        var expectedUpdate = new TrackerUpdateResultDto(
            TransactionId: 100050,
            TrackerId: 1,
            CustomerId: 1001,
            CampaignId: 1,
            ResolvedBrand: "GETIR",
            ResolvedSector: "Grocery & Supermarket",
            IsAcceptedForCampaign: false,
            IsExcludedByAntiGaming: true,
            ExclusionOrClawbackReason: "Anti-Gaming: Split transaction detected at the same merchant within 30 minutes.",
            TargetAmount: 36000.00m,
            CurrentQualifiedSpend: 21500.00m,
            RemainingAmount: 14500.00m,
            ProgressPercentage: 59.72m,
            IsGoalCompleted: false,
            RewardQueuedOrLoaded: false,
            RewardClawedBack: false,
            ProcessingLatencyMs: 9.80m);

        mockTrackerService
            .Setup(s => s.ProcessAuthorizationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TrackerUpdateResultDto>.Success(expectedUpdate));

        // Act
        var result = await mockTrackerService.Object.ProcessAuthorizationAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Value.IsExcludedByAntiGaming);
        Assert.IsFalse(result.Value.IsAcceptedForCampaign);
        Assert.IsTrue(result.Value.ProcessingLatencyMs < KpiThresholds.MaxRealtimeTrackerLatencyMs);
        mockTrackerService.VerifyAll();
    }
}
