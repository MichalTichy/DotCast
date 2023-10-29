using DotCast.Infrastructure.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotCast.AudioBookProvider.FileSystem
{
    public class FileSystemAudioBookProviderInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<FileSystemAudioBookProvider>();
        }
    }
}