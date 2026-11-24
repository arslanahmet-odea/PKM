namespace CausalCampaign.Domain.ValueObjects;

/// <summary>
/// Represents an individual feature contribution weight calculated by Explainable AI (XAI).
/// </summary>
/// <param name="Feature">Technical or business feature name.</param>
/// <param name="Impact">Normalized contribution weight on the decision (0.00 - 1.00).</param>
public sealed record FeatureContributionItem(string Feature, decimal Impact);
