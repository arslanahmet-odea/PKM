using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;

namespace CausalCampaign.Domain.Interfaces;

/// <summary>
/// Service contract for the Google.OrTools MILP portfolio optimizer under budget, credit limit, MIS profitability, and monotonicity fairness constraints.
/// </summary>
public interface IMilpPortfolioOptimizer
{
    /// <summary>
    /// Solves the optimal offer assignment across 20 micro-buckets to maximize net incremental economic value.
    /// </summary>
    Result<PortfolioOptimizationResultDto> OptimizeCampaignPortfolio(
        CampaignTemplate campaign,
        IReadOnlyList<CustomerCausalFeatureView> customers,
        IReadOnlyList<CustomerCausalPredictionDto> causalPredictions);
}
