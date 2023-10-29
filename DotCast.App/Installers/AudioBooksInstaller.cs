using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.Infrastructure.BookInfoProvider.DatabazeKnih;
using DotCast.Infrastructure.IoC;
using DotCast.AudioBookProvider.Base;
using DotCast.AudioBookProvider.Combined;
using DotCast.AudioBookProvider.FileSystem;
using DotCast.AudioBookProvider.Postgre;
using DotCast.RssGenerator.FromFiles;

namespace DotCast.App.Installers
{
    public class AudioBooksInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<FileSystemAudioBookProvider>();
            services.AddScoped<PostgreAudioBookInfoProvider>();

            services.AddScoped<IAudioBookInfoProvider>(provider =>
            {
                var combinedProvider = new CombinedAudioBookProvider();
                combinedProvider.AddProvider(provider.GetRequiredService<PostgreAudioBookInfoProvider>());
                combinedProvider.AddProvider(provider.GetRequiredService<FileSystemAudioBookProvider>());

                return combinedProvider;
            });

            services.AddSingleton<IAudioBookFeedProvider, FileSystemAudioBookProvider>();
            services.AddSingleton<IAudioBookDownloader, FileSystemAudioBookProvider>();
            services.AddSingleton<IAudioBookUploader, FileSystemAudioBookProvider>();

            services.AddSingleton<IBookInfoProvider, DatabazeKnihBookInfoProvider>();

            services.AddSingleton<FromFileRssGenerator>();

            services.Configure<FileSystemAudioBookProviderOptions>(configuration.GetSection(nameof(FileSystemAudioBookProviderOptions)));
        }
    }
}
