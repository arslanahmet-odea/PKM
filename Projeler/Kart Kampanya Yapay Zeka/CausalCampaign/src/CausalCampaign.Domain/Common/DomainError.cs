namespace CausalCampaign.Domain.Common;

/// <summary>
/// Strongly-typed error record that carries business rule violations without throwing exceptions.
/// </summary>
/// <param name="Code">Machine-readable unique error code.</param>
/// <param name="Message">Human-readable English error description.</param>
/// <param name="Type">Error category.</param>
public sealed record DomainError(string Code, string Message, ErrorType Type)
{
    /// <summary>Represents a no-error state.</summary>
    public static readonly DomainError None = new(string.Empty, string.Empty, ErrorType.None);

    /// <summary>Creates a validation error.</summary>
    public static DomainError Validation(string code, string message) => new(code, message, ErrorType.Validation);

    /// <summary>Creates a not-found error.</summary>
    public static DomainError NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    /// <summary>Creates a state or concurrency conflict error.</summary>
    public static DomainError Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    /// <summary>Creates an Anti-Gaming violation error.</summary>
    public static DomainError AntiGaming(string code, string message) => new(code, message, ErrorType.AntiGamingViolation);

    /// <summary>Creates an active Kill-Switch error.</summary>
    public static DomainError KillSwitch(string code, string message) => new(code, message, ErrorType.KillSwitchActive);

    /// <summary>Creates an unexpected error.</summary>
    public static DomainError Unexpected(string code, string message) => new(code, message, ErrorType.Unexpected);
}
