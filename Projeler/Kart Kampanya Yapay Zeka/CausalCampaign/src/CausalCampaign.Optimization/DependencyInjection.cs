using CausalCampaign.Domain.Interfaces;
using CausalCampaign.Optimization.Buckets;
using CausalCampaign.Optimization.Bundles;
using CausalCampaign.Optimization.Configuration;
using CausalCampaign.Optimization.Experiment;
using CausalCampaign.Optimization.Solvers;
using Microsoft.Extensions.DependencyInjection;

namespace CausalCampaign.Optimization;

/// <summary>
/// Registers CausalCampaign.Optimization layer services into the Dependency Injection container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCausalCampaignOptimization(
        this IServiceCollection services,
        Action<OptimizationOptions>? configure = null)
    {
        var options = new OptimizationOptions();
        configure?.Invoke(options);

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
        services.AddSingleton<MicroBucketCatalogService>();
        services.AddSingleton<EquivalentOfferBundleBuilder>();
        services.AddSingleton<StratifiedRctHoldoutSplitter>();
        services.AddScoped<IMilpPortfolioOptimizer, MilpPortfolioOptimizer>();

        return services;
    }
}
