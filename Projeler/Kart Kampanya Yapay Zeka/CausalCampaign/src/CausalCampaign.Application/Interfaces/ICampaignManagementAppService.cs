using CausalCampaign.Application.DTOs;
using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.ValueObjects;

namespace CausalCampaign.Application.Interfaces;

/// <summary>
/// Orchestration contract for end-to-end campaign optimization, customer alternative offer selection
/// (Active Preference Learning), emergency Kill-Switch, executive dashboard summary, and Call Center audit.
/// </summary>
public interface ICampaignManagementAppService
{
    /// <summary>
    /// Executes end-to-end Feature Store -&gt; Robinson R-Learner Uplift -&gt; XAI -&gt; Google.OrTools MILP
    /// optimization for the specified campaign and persists assignments to the database.
    /// </summary>
    Task<Result<PortfolioOptimizationResultDto>> RunEndToEndCampaignOptimizationAsync(
        int campaignId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records the customer's selection among the 3 equivalent offers in the mobile app (Active Preference Learning)
    /// and updates the active progress tracker target accordingly.
    /// </summary>
    Task<Result<AlternativeOfferOption>> SelectCustomerOfferPreferenceAsync(
        SelectAlternativeOfferRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates the emergency Kill-Switch for the campaign and halts further tracker updates.
    /// </summary>
    Task<Result> TriggerCampaignKillSwitchAsync(
        TriggerKillSwitchRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the 8 KPIs and budget summary for the Back-Office executive dashboard.
    /// </summary>
    Task<Result<ExecutiveDashboardSummaryDto>> GetExecutiveDashboardSummaryAsync(
        int campaignId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves customer offer, tracker, XAI rationale, and transaction audit details for the Call Center and Mobile Simulator.
    /// </summary>
    Task<Result<CustomerCallCenterAuditDto>> GetCustomerCallCenterAuditAsync(
        int customerId,
        int campaignId,
        CancellationToken cancellationToken = default);
}
