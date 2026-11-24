using CausalCampaign.Application.Interfaces;
using CausalCampaign.Application.Services;
using CausalCampaign.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CausalCampaign.Application;

/// <summary>
/// Registers CausalCampaign.Application layer services and FluentValidation validators into the Dependency Injection container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCausalCampaignApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IRealtimeOfferTrackerService, RealtimeOfferTrackerService>();
        services.AddScoped<ICampaignManagementAppService, CampaignManagementAppService>();

        return services;
    }
}
