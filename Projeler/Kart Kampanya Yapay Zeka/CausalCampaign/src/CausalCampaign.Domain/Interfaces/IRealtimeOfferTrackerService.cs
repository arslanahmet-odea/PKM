using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;

namespace CausalCampaign.Domain.Interfaces;

/// <summary>
/// Service contract for the sub-500 ms real-time ISO 8583 authorization tracker, Anti-Gaming filter, and refund Clawback engine.
/// </summary>
public interface IRealtimeOfferTrackerService
{
    /// <summary>
    /// Processes a live card authorization message, applies Anti-Gaming filters, and atomically updates the spend goal tracker.
    /// </summary>
    Task<Result<TrackerUpdateResultDto>> ProcessAuthorizationAsync(
        AuthorizeTransactionRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a reversal or refund message, deducts from the tracker, and claws back any earned reward if spend drops below target.
    /// </summary>
    Task<Result<TrackerUpdateResultDto>> ProcessReversalOrRefundAsync(
        ReversalTransactionRequestDto request,
        CancellationToken cancellationToken = default);
}
