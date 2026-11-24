namespace CausalCampaign.Domain.Common;

/// <summary>
/// Defines domain and application error categories.
/// </summary>
public enum ErrorType
{
    /// <summary>No error.</summary>
    None = 0,

    /// <summary>Input or business rule validation error (HTTP 400).</summary>
    Validation = 1,

    /// <summary>Requested entity or resource was not found (HTTP 404).</summary>
    NotFound = 2,

    /// <summary>Optimistic concurrency (RowVersion) or state conflict (HTTP 409).</summary>
    Conflict = 3,

    /// <summary>Anti-Gaming (split transaction or cash-like MCC) rule violation (HTTP 422).</summary>
    AntiGamingViolation = 4,

    /// <summary>Campaign is halted via emergency Kill-Switch (HTTP 422).</summary>
    KillSwitchActive = 5,

    /// <summary>Unexpected internal or solver failure (HTTP 500).</summary>
    Unexpected = 6
}
