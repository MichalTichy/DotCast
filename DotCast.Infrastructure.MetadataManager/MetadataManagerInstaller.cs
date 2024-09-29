using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.IoC;

namespace DotCast.Infrastructure.MetadataManager
{
    public class MetadataManagerInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<IMetadataManager, MetadataManager>();
        }
    }
}
