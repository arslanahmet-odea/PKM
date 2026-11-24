using CausalCampaign.CausalAi.Analytics;
using CausalCampaign.CausalAi.Bandit;
using CausalCampaign.CausalAi.Configuration;
using CausalCampaign.CausalAi.Explainability;
using CausalCampaign.CausalAi.FeatureStore;
using CausalCampaign.CausalAi.Recommendation;
using CausalCampaign.CausalAi.Uplift;
using CausalCampaign.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CausalCampaign.CausalAi;

/// <summary>
/// Registers CausalCampaign.CausalAi layer services into the Dependency Injection container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCausalCampaignCausalAi(
        this IServiceCollection services,
        Action<CausalAiOptions>? configure = null)
    {
        var options = new CausalAiOptions();
        configure?.Invoke(options);

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
        services.AddScoped<InflationAdjustedFeatureStore>();
        services.AddSingleton<DormantCardReactivationModel>();
        services.AddSingleton<MatrixFactorizationSectorRecommender>();
        services.AddSingleton<ThompsonSamplingBanditService>();
        services.AddSingleton<BehavioralHdbscanClusterer>();
        services.AddSingleton<IXaiExplanationService, XaiTurkishExplanationService>();
        services.AddSingleton<IDriftAndClusteringService, InflationPsiDriftMonitor>();
        services.AddSingleton<ICupedExperimentEvaluator, CupedExperimentEvaluator>();
        services.AddSingleton<ICausalUpliftEngine, RobinsonRLearnerUpliftEngine>();

        return services;
    }
}
