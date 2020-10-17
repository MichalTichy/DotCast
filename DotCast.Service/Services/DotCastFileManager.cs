using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotCast.Service.Controllers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotCast.Service.Services
{
    public class DotCastFileManager : BackgroundService
    {
        private readonly ILogger<DotCastFileManager> _logger;
        private readonly PodcastProviderSettings settings;

        public DotCastFileManager(ILogger<DotCastFileManager> logger,PodcastProviderSettings settings)
        {
            _logger = logger;
            this.settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running file name maintenance");

                RenameFilesToUrlFriendlyNames();
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        private void RenameFilesToUrlFriendlyNames()
        {
            var rootDirectory = new DirectoryInfo(settings.PodcastsLocation);
            foreach (var directory in rootDirectory.GetDirectories())
            {
                var activityWaitTime = 10;
                var directoryCreationTime = (DateTime.Now - directory.CreationTime);
                var directoryLastWriteTime = (DateTime.Now - directory.LastWriteTime);
                if (directoryCreationTime<TimeSpan.FromMinutes(activityWaitTime) || directoryLastWriteTime<TimeSpan.FromMinutes(activityWaitTime))
                {
                    _logger.LogWarning($"Skipping renaming of {directory} because it's seems to be currently in use.");
                    _logger.LogWarning($"Directory was created {directoryCreationTime.TotalMinutes} ago.");
                    _logger.LogWarning($"Directory was last writen to {directoryLastWriteTime.TotalMinutes} ago.");
                    continue;
                }

                var newDirectoryName = TransformToUrlSafeFileName(directory.Name);
                if (directory.Name != newDirectoryName)
                {
                    _logger.LogInformation($"Renaming {directory.FullName} to {newDirectoryName}");
                    directory.MoveTo(Path.Combine(directory?.Parent?.FullName ?? "", newDirectoryName));
                }

                foreach (var file in directory.EnumerateFiles())
                {
                    var fileName = file.Name;
                    var newFileName = TransformToUrlSafeFileName(fileName);
                    if (fileName != newFileName)
                    {
                        _logger.LogInformation($"Renaming {directory.FullName} to {newFileName}");
                        file.MoveTo(Path.Combine(directory.FullName, newFileName));
                    }
                }
            }
        }
        string TransformToUrlSafeFileName(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace(' ', '_');
        }
    }
}
