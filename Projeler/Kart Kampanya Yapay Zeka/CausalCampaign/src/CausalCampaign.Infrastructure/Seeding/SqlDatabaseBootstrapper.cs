using System.Text.RegularExpressions;
using CausalCampaign.Domain.Common;
using CausalCampaign.Infrastructure.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CausalCampaign.Infrastructure.Seeding;

/// <summary>
/// Executes the T-SQL setup script on the existing SQL Server database to create the 8 CasualCampaign_
/// prefixed tables, indexes, vw_CasualCampaign_CustomerCausalFeatures view, and initial seed data.
/// </summary>
public sealed partial class SqlDatabaseBootstrapper
{
    private readonly DatabaseOptions _options;

    public SqlDatabaseBootstrapper(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Splits CausalCampaignDb_Setup_And_Seed.sql by GO batch separators and executes each batch on the target SQL Server.
    /// </summary>
    /// <param name="sqlScriptPath">Path to the SQL setup script.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<Result<int>> ExecuteSetupScriptAsync(
        string sqlScriptPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sqlScriptPath) || !File.Exists(sqlScriptPath))
        {
            return Result<int>.Failure(DomainError.NotFound(
                "SQL_SCRIPT_NOT_FOUND",
                $"Database setup script was not found at path: {sqlScriptPath}"));
        }

        string scriptContent = await File.ReadAllTextAsync(sqlScriptPath, cancellationToken);
        string[] batches = SplitSqlBatches(scriptContent);

        try
        {
            await using var connection = new SqlConnection(_options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            int executedBatchCount = 0;
            foreach (string batch in batches)
            {
                string trimmedBatch = batch.Trim();
                if (string.IsNullOrWhiteSpace(trimmedBatch))
                {
                    continue;
                }

                await using var command = new SqlCommand(trimmedBatch, connection)
                {
                    CommandTimeout = _options.CommandTimeoutSeconds
                };

                await command.ExecuteNonQueryAsync(cancellationToken);
                executedBatchCount++;
            }

            return Result<int>.Success(executedBatchCount);
        }
        catch (SqlException ex)
        {
            return Result<int>.Failure(DomainError.Unexpected(
                "SQL_BOOTSTRAP_FAILED",
                $"Failed to bootstrap CasualCampaign_ tables on SQL Server (ErrorNumber={ex.Number}): {ex.Message}"));
        }
    }

    /// <summary>
    /// Splits a T-SQL script by standalone GO lines.
    /// </summary>
    public static string[] SplitSqlBatches(string scriptContent)
    {
        if (string.IsNullOrWhiteSpace(scriptContent))
        {
            return Array.Empty<string>();
        }

        return GoBatchSplitterRegex()
            .Split(scriptContent)
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .ToArray();
    }

    [GeneratedRegex(@"^\s*GO(?:\s*--.*)?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoBatchSplitterRegex();
}
