using CausalCampaign.Domain.Interfaces;
using CausalCampaign.PosEnrichment.Caching;
using CausalCampaign.PosEnrichment.Configuration;
using CausalCampaign.PosEnrichment.Layers;
using CausalCampaign.PosEnrichment.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CausalCampaign.PosEnrichment;

/// <summary>
/// Registers CausalCampaign.PosEnrichment layer services into the Dependency Injection container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCausalCampaignPosEnrichment(
        this IServiceCollection services,
        Action<PosEnrichmentOptions>? configure = null)
    {
        var options = new PosEnrichmentOptions();
        configure?.Invoke(options);

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
        services.AddSingleton<PosMerchantCacheManager>();
        services.AddSingleton<DeterministicBrandDictionaryLayer>();
        services.AddSingleton<PaymentFacilitatorRegexLayer>();
        services.AddSingleton<LocalMlMerchantClassifierLayer>();
        services.AddScoped<IPosEnrichmentService, PosEnrichmentService>();

        return services;
    }
}
