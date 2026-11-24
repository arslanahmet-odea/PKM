using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.ValueObjects;

namespace CausalCampaign.Domain.Interfaces;

/// <summary>
/// Service contract for generating Explainable AI (XAI) decision rationale for the mobile app and Call Center.
/// </summary>
public interface IXaiExplanationService
{
    /// <summary>
    /// Produces the top 3 contributing features and a human-readable explanation text for the assigned target and reward.
    /// </summary>
    (string ExplanationText, IReadOnlyList<FeatureContributionItem> TopContributions) GenerateCustomerExplanation(
        CustomerCausalFeatureView customer,
        decimal assignedTarget,
        decimal assignedReward,
        string rewardType);
}
