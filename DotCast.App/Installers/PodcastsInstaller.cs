using DotCast.Infrastructure.IoC;
using DotCast.PodcastProvider.Base;
using DotCast.PodcastProvider.FileSystem;
using DotCast.RssGenerator.FromFiles;

namespace DotCast.App.Installers
{
    public class PodcastsInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<FileSystemPodcastProvider>();

            services.AddSingleton<IPodcastInfoProvider, FileSystemPodcastProvider>();
            services.AddSingleton<IPodcastFeedProvider, FileSystemPodcastProvider>();
            services.AddSingleton<IPodcastDownloader, FileSystemPodcastProvider>();
            services.AddSingleton<IPodcastUploader, FileSystemPodcastProvider>();

            services.AddSingleton<FromFileRssGenerator>();

            services.Configure<FileSystemPodcastProviderOptions>(configuration.GetSection(nameof(FileSystemPodcastProviderOptions)));
        }
    }
}