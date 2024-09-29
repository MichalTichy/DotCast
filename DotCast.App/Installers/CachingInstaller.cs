using Shared.Infrastructure.IoC;

namespace DotCast.App.Installers
{
    public class CachingInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddMemoryCache();
        }
    }
}
