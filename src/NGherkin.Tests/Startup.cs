using Microsoft.Extensions.DependencyInjection;

namespace NGherkin.Tests;

internal sealed class Startup : StartupBase
{
    public override void Configure(IServiceCollection services)
    {
        services.AddGherkinFeatures();
        services.AddGherkinSteps();
    }
}
