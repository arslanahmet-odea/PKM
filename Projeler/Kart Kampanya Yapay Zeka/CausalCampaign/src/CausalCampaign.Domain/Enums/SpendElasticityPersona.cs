namespace CausalCampaign.Domain.Enums;

/// <summary>
/// Specifies the customer's causal treatment response archetype (persona).
/// </summary>
public enum SpendElasticityPersona
{
    /// <summary>Persuadable: Produces significant net incremental spend (positive uplift) when offered a campaign.</summary>
    Persuadable = 1,

    /// <summary>Free-Rider (Sure Thing): Spends high amounts organically even without a campaign offer.</summary>
    FreeRider_SureThing = 2,

    /// <summary>Dormant Sleeper: Inactive for 90+ days and requires a dedicated reactivation threshold.</summary>
    Dormant_Sleeper = 3,

    /// <summary>Lost Cause: Produces no incremental spend or has negative MIS profitability.</summary>
    LostCause = 4
}
