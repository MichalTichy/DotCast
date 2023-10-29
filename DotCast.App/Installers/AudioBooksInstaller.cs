using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.Infrastructure.BookInfoProvider.DatabazeKnih;
using DotCast.Infrastructure.IoC;
using DotCast.AudioBookProvider.Base;
using DotCast.AudioBookProvider.FileSystem;
using DotCast.AudioBookProvider.Postgre;
using DotCast.RssGenerator.FromFiles;

namespace DotCast.App.Installers
{
    public class AudioBooksInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddTransient<IAudioBookInfoProvider>(provider => provider.GetRequiredService<PostgreAudioBookInfoProvider>());

            services.AddSingleton<IAudioBookFeedProvider, FileSystemAudioBookProvider>();
            services.AddSingleton<IAudioBookDownloader, FileSystemAudioBookProvider>();
            services.AddSingleton<IAudioBookUploader, FileSystemAudioBookProvider>();

            services.AddSingleton<IBookInfoProvider, DatabazeKnihBookInfoProvider>();

            services.AddSingleton<FromFileRssGenerator>();

            services.Configure<FileSystemAudioBookProviderOptions>(configuration.GetSection(nameof(FileSystemAudioBookProviderOptions)));
        }
    }
}
