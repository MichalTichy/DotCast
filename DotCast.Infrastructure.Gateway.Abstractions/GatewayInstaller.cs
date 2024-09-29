using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.IoC;

namespace DotCast.Infrastructure.Gateway.Abstractions
{
    public class GatewayInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.Configure<GatewayOptions>(configuration.GetSection(nameof(GatewayOptions)));
        }
    }
}