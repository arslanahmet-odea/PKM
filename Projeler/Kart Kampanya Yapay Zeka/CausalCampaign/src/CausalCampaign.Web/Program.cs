using CausalCampaign.Application;
using CausalCampaign.CausalAi;
using CausalCampaign.Infrastructure;
using CausalCampaign.Optimization;
using CausalCampaign.PosEnrichment;
using CausalCampaign.Web.Components;
using CausalCampaign.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Dependency Injection Registration for Clean Architecture Layers
builder.Services.AddCausalCampaignInfrastructure(builder.Configuration);
builder.Services.AddCausalCampaignPosEnrichment();
builder.Services.AddCausalCampaignCausalAi();
builder.Services.AddCausalCampaignOptimization();
builder.Services.AddCausalCampaignApplication();

// 2. Centralized Global Error Handling (RFC 7807 ProblemDetails)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. REST API Controllers and Blazor Interactive Server Components
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
