using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;

namespace CausalCampaign.Domain.Interfaces;

/// <summary>
/// Service contract for the two-stage Robinson R-Learner causal inference, Empirical Bayes shrinkage, and dormant card reactivation engine.
/// </summary>
public interface ICausalUpliftEngine
{
    /// <summary>
    /// Predicts expected organic spend, individual treatment effect (uplift), and reactivation probability across customer feature vectors.
    /// </summary>
    Result<IReadOnlyList<CustomerCausalPredictionDto>> PredictPortfolioCausalUplift(
        IReadOnlyList<CustomerCausalFeatureView> customerFeatures,
        CampaignTemplate campaign);
}
