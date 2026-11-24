using System.ComponentModel.DataAnnotations;

namespace CausalCampaign.Infrastructure.Configuration;

/// <summary>
/// Options class holding SQL Server connection and table prefix settings validated at application startup (ValidateOnStart).
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// SQL Server connection string read from the environment variable (.env -> CAUSALCAMPAIGN_DB_CONNECTION).
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SQL Server ConnectionString (CAUSALCAMPAIGN_DB_CONNECTION) cannot be empty.")]
    [MinLength(15, ErrorMessage = "A valid SQL Server ConnectionString must be provided.")]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Table name prefix in the target database (Default: CasualCampaign_).
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string TablePrefix { get; set; } = "CasualCampaign_";

    /// <summary>
    /// EF Core SQL command timeout duration in seconds.
    /// </summary>
    [Range(5, 300, ErrorMessage = "CommandTimeoutSeconds must be between 5 and 300 seconds.")]
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enables sensitive data logging in development environments.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; }
}
