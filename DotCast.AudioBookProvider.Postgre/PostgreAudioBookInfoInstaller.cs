using DotCast.Infrastructure.IoC;
using DotCast.Infrastructure.Persistence.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotCast.AudioBookProvider.Postgre
{
    public class PostgreAudioBookInfoInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddTransient<PostgreAudioBookInfoProvider>();

            services.AddTransient<IStorageConfiguration, AudioBookInfoStorageConfiguration>();
        }
    }
}
