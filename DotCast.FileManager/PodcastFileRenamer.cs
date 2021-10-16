using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DotCast.FileManager
{
    public class PodcastFileNameManager : IPodcastFileNameManager
    {
        private readonly ILogger logger;

        public PodcastFileNameManager(ILogger<PodcastFileNameManager> logger)
        {
            this.logger = logger;
        }

        public void RenameFilesToUrlFriendlyNames(string targetDirectory, int? minimumDirectoryInactivityTimeInMinutes = 10)
        {
            var rootDirectory = new DirectoryInfo(targetDirectory);
            foreach (var directory in rootDirectory.GetDirectories())
            {
                if (minimumDirectoryInactivityTimeInMinutes.HasValue)
                {
                    var activityWaitTime = minimumDirectoryInactivityTimeInMinutes.Value;
                    var directoryCreationTime = (DateTime.Now - directory.CreationTime);
                    var directoryLastWriteTime = (DateTime.Now - directory.LastWriteTime);
                    if (directoryCreationTime < TimeSpan.FromMinutes(activityWaitTime) || directoryLastWriteTime < TimeSpan.FromMinutes(activityWaitTime))
                    {
                        logger.LogWarning($"Skipping renaming of {directory} because it's seems to be currently in use.");
                        logger.LogWarning($"Directory was created {directoryCreationTime.TotalMinutes} ago.");
                        logger.LogWarning($"Directory was last written to {directoryLastWriteTime.TotalMinutes} ago.");
                        continue;
                    }
                }

                var newDirectoryName = TransformToUrlSafeFileName(directory.Name);
                if (directory.Name != newDirectoryName)
                {
                    logger.LogInformation($"Renaming {directory.FullName} to {newDirectoryName}");
                    directory.MoveTo(Path.Combine(directory?.Parent?.FullName ?? "", newDirectoryName));
                }

                foreach (var file in directory.EnumerateFiles())
                {
                    var fileName = file.Name;
                    var newFileName = TransformToUrlSafeFileName(fileName);
                    if (fileName != newFileName)
                    {
                        logger.LogInformation($"Renaming {directory.FullName} to {newFileName}");
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
