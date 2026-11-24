using System.Text.Json;
using CausalCampaign.Application.DTOs;
using CausalCampaign.Application.Interfaces;
using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Interfaces;
using CausalCampaign.Domain.ValueObjects;
using CausalCampaign.Web.Controllers;
using CausalCampaign.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CausalCampaign.UnitTests.WebTests;

[TestClass]
public class WebLayerExtendedTests
{
    [TestMethod]
    public async Task GlobalExceptionHandler_HandlesCancellationAs499_AndSanitizesErrorDetailsWithoutLeakingInternals()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        // 1. Client cancellation -> 499
        var cancelCtx = new DefaultHttpContext();
        cancelCtx.Request.Path = "/api/v1/transactions/authorize";
        bool cancelHandled = await handler.TryHandleAsync(cancelCtx, new OperationCanceledException("Client disconnected"), CancellationToken.None);
        Assert.IsTrue(cancelHandled);
        Assert.AreEqual(499, cancelCtx.Response.StatusCode);

        // 2. ArgumentException -> 400 with sanitized message (must NOT leak internal secret/parameter detail)
        var argCtx = new DefaultHttpContext();
        argCtx.Request.Path = "/api/v1/campaigns/1/dashboard";
        argCtx.Response.Body = new MemoryStream();
        await handler.TryHandleAsync(argCtx, new ArgumentException("INTERNAL_SECRET_TABLE_NAME leaked"), CancellationToken.None);
        Assert.AreEqual(StatusCodes.Status400BadRequest, argCtx.Response.StatusCode);
        Assert.IsTrue(argCtx.Response.ContentType!.StartsWith("application/problem+json", StringComparison.OrdinalIgnoreCase));

        argCtx.Response.Body.Position = 0;
        string body = await new StreamReader(argCtx.Response.Body).ReadToEndAsync();
        Assert.IsFalse(body.Contains("INTERNAL_SECRET_TABLE_NAME"), "Internal exception message must not be leaked in ProblemDetails.");
        Assert.IsTrue(body.Contains("traceId"));

        // 3. KeyNotFoundException -> 404
        var notFoundCtx = new DefaultHttpContext();
        notFoundCtx.Response.Body = new MemoryStream();
        await handler.TryHandleAsync(notFoundCtx, new KeyNotFoundException("Missing"), CancellationToken.None);
        Assert.AreEqual(StatusCodes.Status404NotFound, notFoundCtx.Response.StatusCode);

        // 4. Generic Exception -> 500
        var serverCtx = new DefaultHttpContext();
        serverCtx.Response.Body = new MemoryStream();
        await handler.TryHandleAsync(serverCtx, new InvalidOperationException("DB crash"), CancellationToken.None);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, serverCtx.Response.StatusCode);
    }

    [TestMethod]
    public async Task CampaignEngineController_CoversAllEndpointsAndErrorStatusCodes()
    {
        var mockTracker = new Mock<IRealtimeOfferTrackerService>(MockBehavior.Strict);
        var mockApp = new Mock<ICampaignManagementAppService>(MockBehavior.Strict);
        var controller = new CampaignEngineController(mockTracker.Object, mockApp.Object);

        // ReverseTransaction -> 409 Conflict on duplicate reversal
        var revReq = new ReversalTransactionRequestDto("REV-1", 1001, 1, 10, 500m, DateTime.UtcNow);
        mockTracker.Setup(s => s.ProcessReversalOrRefundAsync(revReq, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TrackerUpdateResultDto>.Failure(DomainError.Conflict("DUPLICATE_REVERSAL_CODE", "Duplicate")));
        var revRes = (await controller.ReverseTransaction(revReq, CancellationToken.None)).Result as ObjectResult;
        Assert.AreEqual(StatusCodes.Status409Conflict, revRes?.StatusCode);

        // SelectOfferPreference -> 200 OK
        var selectReq = new SelectAlternativeOfferRequestDto(1, 1001, 2);
        mockApp.Setup(s => s.SelectCustomerOfferPreferenceAsync(selectReq, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AlternativeOfferOption>.Success(new AlternativeOfferOption(2, "Cashback", 20000m, 450m, "Cashback")));
        var selectRes = (await controller.SelectOfferPreference(selectReq, CancellationToken.None)).Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, selectRes?.StatusCode);

        // OptimizeCampaign -> 422 UnprocessableEntity when Kill-Switch is active
        mockApp.Setup(s => s.RunEndToEndCampaignOptimizationAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PortfolioOptimizationResultDto>.Failure(DomainError.KillSwitch("CAMPAIGN_KILL_SWITCH_ACTIVE", "Halted")));
        var optRes = (await controller.OptimizeCampaign(1, CancellationToken.None)).Result as ObjectResult;
        Assert.AreEqual(StatusCodes.Status422UnprocessableEntity, optRes?.StatusCode);

        // TriggerKillSwitch -> 200 OK
        var killReq = new TriggerKillSwitchRequestDto(1, "Emergency drift stop", "RiskAdmin");
        mockApp.Setup(s => s.TriggerCampaignKillSwitchAsync(killReq, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var killRes = await controller.TriggerKillSwitch(killReq, CancellationToken.None) as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, killRes?.StatusCode);

        // GetDashboardSummary -> 404 NotFound
        mockApp.Setup(s => s.GetExecutiveDashboardSummaryAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExecutiveDashboardSummaryDto>.Failure(DomainError.NotFound("CAMPAIGN_NOT_FOUND", "Not found")));
        var dashRes = (await controller.GetDashboardSummary(999, CancellationToken.None)).Result as ObjectResult;
        Assert.AreEqual(StatusCodes.Status404NotFound, dashRes?.StatusCode);

        // GetCustomerAudit -> 404 NotFound
        mockApp.Setup(s => s.GetCustomerCallCenterAuditAsync(999, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CustomerCallCenterAuditDto>.Failure(DomainError.NotFound("CUSTOMER_NOT_FOUND", "Not found")));
        var auditRes = (await controller.GetCustomerAudit(999, 1, CancellationToken.None)).Result as ObjectResult;
        Assert.AreEqual(StatusCodes.Status404NotFound, auditRes?.StatusCode);

        mockTracker.VerifyAll();
        mockApp.VerifyAll();
    }
}
