using CausalCampaign.Application.Interfaces;
using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Interfaces;
using CausalCampaign.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CausalCampaign.UnitTests.WebTests;

[TestClass]
public class CampaignEngineControllerTests
{
    [TestMethod]
    public async Task AuthorizeTransaction_WhenServiceSucceeds_Returns200OkWithTrackerUpdate()
    {
        // Arrange
        var mockTracker = new Mock<IRealtimeOfferTrackerService>(MockBehavior.Strict);
        var mockApp = new Mock<ICampaignManagementAppService>(MockBehavior.Strict);

        var request = new AuthorizeTransactionRequestDto(
            "AUTH-5001", 1001, 1, 4500m, "TRY", "5399", "IYZICO*TRENDYOL.COM", DateTime.UtcNow);

        var expectedResponse = new TrackerUpdateResultDto(
            100010, 1, 1001, 1, "TRENDYOL", "E-Commerce & Marketplace", true, false, null,
            36000m, 26000m, 10000m, 72.22m, false, false, false, 8.20m);

        mockTracker
            .Setup(s => s.ProcessAuthorizationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TrackerUpdateResultDto>.Success(expectedResponse));

        var controller = new CampaignEngineController(mockTracker.Object, mockApp.Object);

        // Act
        var actionResult = await controller.AuthorizeTransaction(request, CancellationToken.None);

        // Assert
        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        var dto = okResult.Value as TrackerUpdateResultDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual("TRENDYOL", dto.ResolvedBrand);
        mockTracker.VerifyAll();
    }

    [TestMethod]
    public async Task AuthorizeTransaction_WhenKillSwitchIsActive_Returns422UnprocessableEntityProblemDetails()
    {
        // Arrange
        var mockTracker = new Mock<IRealtimeOfferTrackerService>(MockBehavior.Strict);
        var mockApp = new Mock<ICampaignManagementAppService>(MockBehavior.Strict);

        var request = new AuthorizeTransactionRequestDto(
            "AUTH-5002", 1001, 1, 4500m, "TRY", "5399", "IYZICO*TRENDYOL.COM", DateTime.UtcNow);

        mockTracker
            .Setup(s => s.ProcessAuthorizationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TrackerUpdateResultDto>.Failure(
                DomainError.KillSwitch("CAMPAIGN_KILL_SWITCH_ACTIVE", "Campaign has been halted via Kill-Switch.")));

        var controller = new CampaignEngineController(mockTracker.Object, mockApp.Object);

        // Act
        var actionResult = await controller.AuthorizeTransaction(request, CancellationToken.None);

        // Assert
        var objResult = actionResult.Result as ObjectResult;
        Assert.IsNotNull(objResult);
        Assert.AreEqual(StatusCodes.Status422UnprocessableEntity, objResult.StatusCode);
        var problem = objResult.Value as ProblemDetails;
        Assert.IsNotNull(problem);
        Assert.AreEqual("CAMPAIGN_KILL_SWITCH_ACTIVE", problem.Title);
    }
}
