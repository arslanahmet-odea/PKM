namespace CausalCampaign.Infrastructure.Configuration;

/// <summary>
/// Utility class that loads environment variables from a .env file without hardcoding secrets in source code.
/// </summary>
public static class DotEnvLoader
{
    private const int MaxHierarchyDepth = 8;

    /// <summary>
    /// Searches the current and up to 8 parent directories for a .env file and loads its key-value pairs into process environment variables.
    /// </summary>
    /// <param name="startDirectory">Starting directory for the search (defaults to AppContext.BaseDirectory).</param>
    /// <returns>The full path of the loaded .env file if found; otherwise null.</returns>
    public static string? LoadFromHierarchy(string? startDirectory = null)
    {
        var current = new DirectoryInfo(startDirectory ?? AppContext.BaseDirectory);
        int depth = 0;

        while (current is not null && depth <= MaxHierarchyDepth)
        {
            string candidatePath = Path.Combine(current.FullName, ".env");
            if (File.Exists(candidatePath))
            {
                LoadFile(candidatePath);
                return candidatePath;
            }

            current = current.Parent;
            depth++;
        }

        return null;
    }

    /// <summary>
    /// Parses the .env file at the specified path and populates process environment variables.
    /// </summary>
    public static void LoadFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        foreach (string rawLine in File.ReadAllLines(filePath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
            {
                line = line[7..].TrimStart();
            }

            int separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            string key = line[..separatorIndex].Trim();
            string value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) ||
                 (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
