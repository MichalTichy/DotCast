using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotCast.Infrastructure.IoC;

namespace DotCast.Infrastructure.UrlBuilder;

public class UrlBuilderInstaller : IInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.Configure<UrlBuilderOptions>(configuration.GetSection(nameof(UrlBuilderOptions)));
        services.AddSingleton<IUrlBuilder, UrlBuilder>();
    }
}
