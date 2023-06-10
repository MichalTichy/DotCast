using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.Infrastructure.BookInfoProvider.DatabazeKnih;
using DotCast.Infrastructure.IoC;
using DotCast.PodcastProvider.Base;
using DotCast.PodcastProvider.Combined;
using DotCast.PodcastProvider.FileSystem;
using DotCast.PodcastProvider.Postgre;
using DotCast.RssGenerator.FromFiles;

namespace DotCast.App.Installers
{
    public class PodcastsInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<FileSystemPodcastProvider>();
            services.AddScoped<PostgrePodcastInfoProvider>();

            services.AddScoped<IPodcastInfoProvider>(provider =>
            {
                var combinedProvider = new CombinedPodcastProvider();
                combinedProvider.AddProvider(provider.GetRequiredService<PostgrePodcastInfoProvider>());
                combinedProvider.AddProvider(provider.GetRequiredService<FileSystemPodcastProvider>());

                return combinedProvider;
            });

            services.AddSingleton<IPodcastFeedProvider, FileSystemPodcastProvider>();
            services.AddSingleton<IPodcastDownloader, FileSystemPodcastProvider>();
            services.AddSingleton<IPodcastUploader, FileSystemPodcastProvider>();

            services.AddSingleton<IBookInfoProvider, DatabazeKnihBookInfoProvider>();

            services.AddSingleton<FromFileRssGenerator>();

            services.Configure<FileSystemPodcastProviderOptions>(configuration.GetSection(nameof(FileSystemPodcastProviderOptions)));
        }
    }
}