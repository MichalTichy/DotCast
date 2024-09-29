using DotCast.Storage.Abstractions;
using DotCast.Storage.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.IoC;

namespace DotCast.Storage
{
    public class StorageInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<IStorage, SimpleStorage>();
            services.AddSingleton<IStorageApiInformationProvider, StorageApiInformationProvider>();
            services.Configure<StorageOptions>(configuration.GetSection(nameof(StorageOptions)));
            services.AddSingleton<IFilesystemPathManager, FilesystemPathManager>();
        }
    }
}