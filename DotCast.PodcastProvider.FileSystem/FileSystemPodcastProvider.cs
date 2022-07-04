﻿using System.IO.Compression;
using DotCast.PodcastProvider.Base;
using DotCast.RssGenerator.FromFiles;
using Microsoft.Extensions.Options;

namespace DotCast.PodcastProvider.FileSystem
{
    public class FileSystemPodcastProvider : IPodcastInfoProvider, IPodcastFeedProvider, IPodcastUploader, IPodcastDownloader
    {
        private readonly IOptions<FileSystemPodcastProviderOptions> options;
        private readonly FromFileRssGenerator rssGenerator;
        private FileSystemPodcastProviderOptions settings => options.Value;

        public FileSystemPodcastProvider(IOptions<FileSystemPodcastProviderOptions> options, FromFileRssGenerator rssGenerator)
        {
            this.options = options;
            this.rssGenerator = rssGenerator;
        }

        public string GetRss(string name)
        {
            var podcastName = GetNormalizedName(name);

            var podcastPath = Path.Combine(settings.PodcastsLocation, name);
            var directory = new DirectoryInfo(podcastPath);
            var filePaths = directory.GetFiles().Select(t => new LocalFileInfo(t.FullName, GetFileUrl(t.Directory!.Name, t.Name))).ToArray();

            var rssGeneratorParams = new RssFromFileParams(podcastName, filePaths);

            return rssGenerator.GenerateRss(rssGeneratorParams);
        }

        public IEnumerable<PodcastInfo> GetPodcasts()
        {
            var baseDirectory = new DirectoryInfo(settings.PodcastsLocation);
            foreach (var directory in baseDirectory.GetDirectories())
            {
                var podcastName = GetNormalizedName(directory.Name);

                var filePaths = directory.GetFiles().Select(t => new LocalFileInfo(t.FullName, GetFileUrl(t.Directory!.Name, t.Name))).ToArray();

                var rssGeneratorParams = new RssFromFileParams(podcastName, filePaths);

                var feed = rssGenerator.BuildFeed(rssGeneratorParams);

                yield return new PodcastInfo(directory.Name, feed.Title, feed.AuthorName ?? "Unknown author", $"{settings.PodcastServerUrl}/podcast/{directory.Name}", feed.ImageUrl, feed.Duration);
            }
        }

        private static string GetNormalizedName(string directory)
        {
            return directory.Replace("_", " ");
        }

        private static string GetEscapedName(string directory)
        {
            return directory.Replace(" ", "_");
        }

        private string GetFileUrl(string directoryName, string fileName)
        {
            return $"{settings.PodcastServerUrl}/files/{directoryName}/{fileName}";
        }

        private string GetZipUrl(string fileName)
        {
            return $"{settings.PodcastServerUrl}/zip/{fileName}";
        }


        public async Task GenerateZip(string podcastId, bool replace = false)
        {
            var path = GetPodcastZipPath(podcastId);
            if (!replace && File.Exists(path))
            {
                return;
            }

            var podcastDirectory = GetPodcastDirectory(podcastId);
            var files = Directory.GetFiles(podcastDirectory);

            var tmpSuffix = ".tmp";

            var tmpPath = $"{path}{tmpSuffix}";
            await using (var zipFileStream = GetPodcastZipWriteStream(tmpPath))
            {
                using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Update, true);

                foreach (var botFilePath in files)
                {
                    var name = Path.GetFileName(botFilePath);
                    var entry = archive.CreateEntry(name);
                    await using var entryStream = entry.Open();
                    await using var fileStream = File.OpenRead(botFilePath);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            File.Move(tmpPath, path);
        }


        public bool IsDownloadSupported(string podcastId)
        {
            return File.Exists(GetPodcastZipPath(podcastId));
        }

        public Task<string> GetZipDownloadUrl(string podcastId)
        {
            var podcastZipPath = GetPodcastZipPath(podcastId);

            if (!File.Exists(podcastZipPath))
            {
                throw new ArgumentException("Unable to find requested zip");
            }

            var fileName = Path.GetFileName(podcastZipPath);
            return Task.FromResult(GetZipUrl(fileName));
        }

        public FileStream GetPodcastWriteStream(string podcastName, string fileName, string fileContentType)
        {
            var finalDirectoryPath = GetPodcastDirectory(podcastName);

            var targetFileName = GetEscapedName(fileName);
            var fileFilePath = Path.Combine(finalDirectoryPath, targetFileName);
            if (!Directory.Exists(finalDirectoryPath))
            {
                Directory.CreateDirectory(finalDirectoryPath);
            }

            return new FileStream(fileFilePath, FileMode.Create);
        }

        private string GetPodcastDirectory(string podcastId)
        {
            var targetDirectoryName = podcastId;
            var finalDirectoryPath = Path.Combine(options.Value.PodcastsLocation, targetDirectoryName);
            return finalDirectoryPath;
        }

        public FileStream GetPodcastZipWriteStream(string filePath)
        {
            if (!Directory.Exists(options.Value.ZippedPodcastsLocation))
            {
                Directory.CreateDirectory(options.Value.ZippedPodcastsLocation);
            }

            return new FileStream(filePath, FileMode.Create);
        }

        private string GetPodcastZipPath(string podcastId)
        {
            var targetFileName = podcastId;
            var fileFilePath = Path.Combine(options.Value.ZippedPodcastsLocation, targetFileName);
            return $"{fileFilePath}.zip";
        }
    }
}