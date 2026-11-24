namespace CausalCampaign.Domain.Enums;

/// <summary>
/// Specifies the loading and clawback status of an earned campaign reward.
/// </summary>
public enum FulfillmentStatus
{
    /// <summary>Target completed; awaiting end-of-day clearing and settlement confirmation.</summary>
    PendingSettlement = 1,

    /// <summary>Reward has been loaded onto the customer's card.</summary>
    Loaded = 2,

    /// <summary>Total spend dropped below target due to a refund/reversal and the reward was clawed back.</summary>
    ClawedBack = 3
}
