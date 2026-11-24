namespace CausalCampaign.Domain.Common;

/// <summary>
/// Base audit trail abstraction for all domain entities.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Record creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
