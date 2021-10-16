using System;
using System.Threading;
using System.Threading.Tasks;
using DotCast.FileManager;
using DotCast.Service.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotCast.Service.Services
{
    public class DotCastFileManager : BackgroundService
    {
        private readonly ILogger<DotCastFileManager> logger;
        private readonly PodcastProviderSettings settings;
        private readonly IPodcastFileNameManager podcastFileNameManager;

        public DotCastFileManager(ILogger<DotCastFileManager> logger, PodcastProviderSettings settings, IPodcastFileNameManager podcastFileNameManager)
        {
            this.logger = logger;
            this.settings = settings;
            this.podcastFileNameManager = podcastFileNameManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Running file name maintenance");
                podcastFileNameManager.RenameFilesToUrlFriendlyNames(settings.PodcastsLocation, 10);
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
