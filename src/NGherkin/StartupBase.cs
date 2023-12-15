using Microsoft.Extensions.DependencyInjection;

namespace NGherkin;

public abstract class StartupBase
{
    public abstract void Configure(IServiceCollection services);
}
