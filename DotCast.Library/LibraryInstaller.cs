using DotCast.Library.RSS;
using DotCast.Library.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.IoC;
using Shared.Infrastructure.Persistence.Marten.StorageConfiguration;

namespace DotCast.Library
{
    public class LibraryInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddTransient<IStorageConfiguration, AudioBookStorageConfiguration>();
            services.AddSingleton<AudioBookRssGenerator>();
        }
    }
}