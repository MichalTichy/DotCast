using DotCast.Infrastructure.IoC;
using DotCast.Infrastructure.Persistence.Marten;
using DotCast.Library.RSS;
using DotCast.Library.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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