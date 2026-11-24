using CausalCampaign.Domain.Interfaces;
using CausalCampaign.Infrastructure.Configuration;
using CausalCampaign.Infrastructure.Persistence;
using CausalCampaign.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CausalCampaign.Infrastructure;

/// <summary>
/// Registers CausalCampaign.Infrastructure layer services into the Dependency Injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers EF Core DbContext, DatabaseOptions validation, and seeding services.
    /// </summary>
    public static IServiceCollection AddCausalCampaignInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        DotEnvLoader.LoadFromHierarchy();

        string? envConnectionString = Environment.GetEnvironmentVariable("CAUSALCAMPAIGN_DB_CONNECTION");
        string? envTablePrefix = Environment.GetEnvironmentVariable("CAUSALCAMPAIGN_TABLE_PREFIX");

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.ConnectionString) && !string.IsNullOrWhiteSpace(envConnectionString))
                {
                    options.ConnectionString = envConnectionString;
                }

                if (!string.IsNullOrWhiteSpace(envTablePrefix))
                {
                    options.TablePrefix = envTablePrefix;
                }
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<CausalCampaignDbContext>((sp, dbOptions) =>
        {
            var options = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            dbOptions.UseSqlServer(options.ConnectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(options.CommandTimeoutSeconds);
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            });

            if (options.EnableSensitiveDataLogging)
            {
                dbOptions.EnableSensitiveDataLogging();
            }
        });

        services.AddScoped<ICausalCampaignDbContext>(sp => sp.GetRequiredService<CausalCampaignDbContext>());
        services.AddScoped<SqlDatabaseBootstrapper>();
        services.AddScoped<EfCoreSyntheticPortfolioGenerator>();

        return services;
    }
}
