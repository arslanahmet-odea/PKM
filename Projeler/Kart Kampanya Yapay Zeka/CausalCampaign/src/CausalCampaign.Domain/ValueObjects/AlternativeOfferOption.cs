namespace CausalCampaign.Domain.ValueObjects;

/// <summary>
/// Represents an equivalent-profit alternative offer option presented to the customer in the mobile app
/// under Active Preference Learning.
/// </summary>
/// <param name="Index">Option index (1, 2, or 3).</param>
/// <param name="Title">Offer title displayed to the customer.</param>
/// <param name="Target">Spend target amount in this option (TRY).</param>
/// <param name="Reward">Reward amount in this option (TRY).</param>
/// <param name="RewardType">Reward type ("RewardPoints" or "Cashback").</param>
public sealed record AlternativeOfferOption(
    byte Index,
    string Title,
    decimal Target,
    decimal Reward,
    string RewardType);
