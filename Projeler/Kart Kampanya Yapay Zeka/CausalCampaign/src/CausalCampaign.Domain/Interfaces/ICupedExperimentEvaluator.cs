using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;

namespace CausalCampaign.Domain.Interfaces;

/// <summary>
/// Service contract for verifying net incremental economic value (Incremental ROI) via Stratified RCT and CUPED variance reduction.
/// </summary>
public interface ICupedExperimentEvaluator
{
    /// <summary>
    /// Calculates CUPED-adjusted Incremental ROI from pre-experiment and post-experiment spend across Treatment and Control groups.
    /// </summary>
    Result<CupedEvaluationResultDto> EvaluateCampaignIncrementalRoi(
        int campaignId,
        decimal marginRate,
        IReadOnlyList<(decimal PreExperimentSpend, decimal PostExperimentSpend, decimal RewardCost, bool IsTreatment)> observations);
}
