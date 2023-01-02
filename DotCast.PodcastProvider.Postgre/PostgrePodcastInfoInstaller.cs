using DotCast.Infrastructure.IoC;
using DotCast.Infrastructure.Persistence.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotCast.PodcastProvider.Postgre
{
    public class PostgrePodcastInfoInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddTransient<IStorageConfiguration, PodcastInfoStorageConfiguration>();
        }
    }
}