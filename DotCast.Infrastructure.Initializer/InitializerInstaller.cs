using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotCast.Infrastructure.IoC;

namespace DotCast.Infrastructure.Initializer;

public class InitializerInstaller : IInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.AddSingleton<InitializerManager>();

        //installers are added to service collection from startups.
    }
}
