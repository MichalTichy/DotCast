using DotCast.PodcastProvider.Base;
using DotCast.PodcastProvider.CachedProvider;
using DotCast.PodcastProvider.FileSystem;
using DotCast.RssGenerator.FromFiles;

namespace DotCast.App
{
    public static class Installer
    {
        public static IServiceCollection InstallApp(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<FileSystemPodcastProvider>();
            serviceCollection.AddSingleton<IPodcastInfoProvider, CachedPodcastProvider<FileSystemPodcastProvider>>();
            serviceCollection.AddSingleton<IPodcastFeedProvider, CachedPodcastProvider<FileSystemPodcastProvider>>();
            serviceCollection.AddSingleton<IPodcastDownloader, FileSystemPodcastProvider>();
            serviceCollection.AddSingleton<IPodcastUploader, FileSystemPodcastProvider>();
            serviceCollection.AddSingleton<FromFileRssGenerator>();

            serviceCollection.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));
            serviceCollection.Configure<FileSystemPodcastProviderOptions>(configuration.GetSection(nameof(FileSystemPodcastProviderOptions)));

            serviceCollection.AddMemoryCache();
            return serviceCollection;
        }
    }
}