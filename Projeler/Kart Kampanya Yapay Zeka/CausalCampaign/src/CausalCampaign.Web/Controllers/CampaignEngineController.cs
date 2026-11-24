using CausalCampaign.Application.DTOs;
using CausalCampaign.Application.Interfaces;
using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Interfaces;
using CausalCampaign.Domain.ValueObjects;
using CausalCampaign.Web.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace CausalCampaign.Web.Controllers;

/// <summary>
/// REST API endpoints for real-time ISO 8583 card authorization/reversal processing,
/// R-Learner + MILP portfolio optimization, customer alternative offer selection, and Kill-Switch management.
/// </summary>
[ApiController]
[Route("api/v1")]
public sealed class CampaignEngineController : ControllerBase
{
    private readonly IRealtimeOfferTrackerService _trackerService;
    private readonly ICampaignManagementAppService _campaignAppService;

    public CampaignEngineController(
        IRealtimeOfferTrackerService trackerService,
        ICampaignManagementAppService campaignAppService)
    {
        _trackerService = trackerService;
        _campaignAppService = campaignAppService;
    }

    /// <summary>
    /// Processes a real-time ISO 8583 0100/0200 card authorization message (&lt; 500 ms).
    /// </summary>
    [HttpPost("transactions/authorize")]
    public async Task<ActionResult<TrackerUpdateResultDto>> AuthorizeTransaction(
        [FromBody] AuthorizeTransactionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _trackerService.ProcessAuthorizationAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Processes a real-time ISO 8583 0400/0420 reversal or refund message and applies Clawback if needed.
    /// </summary>
    [HttpPost("transactions/reversal")]
    public async Task<ActionResult<TrackerUpdateResultDto>> ReverseTransaction(
        [FromBody] ReversalTransactionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _trackerService.ProcessReversalOrRefundAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Runs Robinson R-Learner Uplift + Google.OrTools MILP portfolio optimization for the specified campaign.
    /// </summary>
    [HttpPost("campaigns/{campaignId:int}/optimize")]
    public async Task<ActionResult<PortfolioOptimizationResultDto>> OptimizeCampaign(
        [FromRoute] int campaignId,
        CancellationToken cancellationToken)
    {
        var result = await _campaignAppService.RunEndToEndCampaignOptimizationAsync(campaignId, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Records the customer's selection among 3 equivalent offers in the mobile app (Active Preference Learning).
    /// </summary>
    [HttpPost("offers/select-preference")]
    public async Task<ActionResult<AlternativeOfferOption>> SelectOfferPreference(
        [FromBody] SelectAlternativeOfferRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _campaignAppService.SelectCustomerOfferPreferenceAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Activates the emergency Kill-Switch for a campaign.
    /// </summary>
    [HttpPost("campaigns/kill-switch")]
    public async Task<IActionResult> TriggerKillSwitch(
        [FromBody] TriggerKillSwitchRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _campaignAppService.TriggerCampaignKillSwitchAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(new { message = "Campaign emergency Kill-Switch activated successfully." });
        }

        var (statusCode, problem) = GlobalExceptionHandler.MapDomainErrorToProblemDetails(result.Error, HttpContext?.Request.Path);
        return StatusCode(statusCode, problem);
    }

    /// <summary>
    /// Returns the Back-Office 8-KPI and budget executive dashboard summary.
    /// </summary>
    [HttpGet("campaigns/{campaignId:int}/dashboard")]
    public async Task<ActionResult<ExecutiveDashboardSummaryDto>> GetDashboardSummary(
        [FromRoute] int campaignId,
        CancellationToken cancellationToken)
    {
        var result = await _campaignAppService.GetExecutiveDashboardSummaryAsync(campaignId, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Returns customer offer, tracker, XAI rationale, and transaction audit details for the Call Center and Mobile Simulator.
    /// </summary>
    [HttpGet("customers/{customerId:int}/campaigns/{campaignId:int}/audit")]
    public async Task<ActionResult<CustomerCallCenterAuditDto>> GetCustomerAudit(
        [FromRoute] int customerId,
        [FromRoute] int campaignId,
        CancellationToken cancellationToken)
    {
        var result = await _campaignAppService.GetCustomerCallCenterAuditAsync(customerId, campaignId, cancellationToken);
        return ToActionResult(result);
    }

    private ActionResult<T> ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        var (statusCode, problem) = GlobalExceptionHandler.MapDomainErrorToProblemDetails(result.Error, HttpContext?.Request.Path);
        return StatusCode(statusCode, problem);
    }
}
