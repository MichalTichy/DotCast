using DotCast.Infrastructure.IoC;
using DotCast.Infrastructure.Persistence.Marten;
using DotCast.Infrastructure.Persistence.Marten.StorageConfiguration;
using DotCast.Library.RSS;
using DotCast.Library.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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