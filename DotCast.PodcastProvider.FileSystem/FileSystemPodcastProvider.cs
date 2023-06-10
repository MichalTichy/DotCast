using System.Globalization;
using System.IO.Compression;
using System.Text;
using DotCast.PodcastProvider.Base;
using DotCast.PodcastProvider.Postgre;
using DotCast.RssGenerator.FromFiles;
using Microsoft.Extensions.Options;
using File = TagLib.File;

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

        public async Task<string> GetRss(string id)
        {
            var rssGeneratorParams = GetFeedParams(id);

            return await rssGenerator.GenerateRss(rssGeneratorParams);
        }

        private RssFromFileParams GetFeedParams(string id)
        {
            var podcastName = GetNormalizedName(id);

            var podcastPath = Path.Combine(settings.PodcastsLocation, id);
            var directory = new DirectoryInfo(podcastPath);
            var filePaths = directory.GetFiles().Select(t => new LocalFileInfo(t.FullName, GetFileUrl(t.Directory!.Name, t.Name))).ToArray();

            var rssGeneratorParams = new RssFromFileParams(podcastName, filePaths);
            return rssGeneratorParams;
        }

        public async Task<string?> GetFeedCover(string id)
        {
            var rssFromFileParams = GetFeedParams(id);
            var feed = await rssGenerator.BuildFeed(rssFromFileParams);
            return feed.ImageUrl;
        }

        public async IAsyncEnumerable<PodcastInfo> GetPodcasts(string? searchText = null)
        {
            var baseDirectory = new DirectoryInfo(settings.PodcastsLocation);
            foreach (var directory in baseDirectory.GetDirectories())
            {
                var podcastInfo = await Get(directory.Name);
                if (podcastInfo == null)
                {
                    continue;
                }

                var matchesSearchText = searchText == null ||
                                        podcastInfo.Name.Contains(searchText) ||
                                        podcastInfo.AuthorName.Contains(searchText) ||
                                        (podcastInfo.SeriesName?.Contains(searchText) ?? false);
                if (!matchesSearchText)
                {
                    continue;
                }

                yield return podcastInfo;
            }
        }

        public Task UpdatePodcastInfo(PodcastInfo podcastInfo)
        {
            _ = Task.Run(() =>
            {
                var localFileInfos = GetFiles(podcastInfo.Id, out _);
                foreach (var localFileInfo in localFileInfos)
                {
                    try
                    {
                        var file = File.Create(localFileInfo.LocalPath);
                        file.Tag.Album = podcastInfo.Name;
                        file.Tag.Performers = new[] {podcastInfo.AuthorName};

                        file.Tag.Grouping = podcastInfo.SeriesName;
                        file.Tag.TitleSort = podcastInfo.OrderInSeries.ToString();

                        file.Tag.Description = podcastInfo.Description;

                        file.Save();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            });

            return Task.CompletedTask;
        }

        public async Task<PodcastInfo?> Get(string id)
        {
            var filePaths = GetFiles(id, out var directory);

            var rssGeneratorParams = new RssFromFileParams(id, filePaths);

            var feed = await rssGenerator.BuildFeed(rssGeneratorParams);

            return new PodcastInfo(directory.Name, feed.Title, feed.AuthorName ?? "Unknown author", null, 0, feed.Description, $"{settings.PodcastServerUrl}/podcast/{directory.Name}", feed.ImageUrl,
                0,feed.Duration);
        }

        public async Task<PodcastsStatistics> GetStatistics()
        {
            var podcasts = await GetPodcasts().ToListAsync();
            return PodcastsStatistics.Create(podcasts);
        }

        private ICollection<LocalFileInfo> GetFiles(string id, out DirectoryInfo directory)
        {
            var path = Path.Combine(settings.PodcastsLocation, id);
            directory = new DirectoryInfo(path);
            var filePaths = directory.GetFiles().Select(t => new LocalFileInfo(t.FullName, GetFileUrl(t.Directory!.Name, t.Name))).ToArray();
            return filePaths;
        }

        public IEnumerable<string> GetPodcastIdsAvailableForDownload()
        {
            return Directory.GetFiles(options.Value.ZippedPodcastsLocation).Select(Path.GetFileNameWithoutExtension).Where(t => t != null).Select(t => t!);
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(normalizedString.Length);

            for (var i = 0; i < normalizedString.Length; i++)
            {
                var c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        private static string GetNormalizedName(string directory)
        {
            return directory.Replace("_", " ");
        }

        private static string GetEscapedName(string name)
        {
            var noSpaces = name;
            return RemoveDiacritics(noSpaces);
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
            if (!replace && System.IO.File.Exists(path))
            {
                return;
            }

            var podcastDirectory = GetPodcastDirectory(podcastId);
            var files = Directory.GetFiles(podcastDirectory);

            var tmpSuffix = ".tmp";

            var tmpPath = $"{path}{tmpSuffix}";
            await using (var zipFileStream = GetPodcastZipWriteStream(tmpPath))
            {
                using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Update);

                foreach (var botFilePath in files)
                {
                    var name = Path.GetFileName(botFilePath);
                    var entry = archive.CreateEntry(name, CompressionLevel.SmallestSize);
                    await using var entryStream = entry.Open();
                    await using var fileStream = System.IO.File.OpenRead(botFilePath);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            System.IO.File.Move(tmpPath, path);
        }


        public bool IsDownloadSupported(string podcastId)
        {
            return System.IO.File.Exists(GetPodcastZipPath(podcastId));
        }

        public Task<string> GetZipDownloadUrl(string podcastId)
        {
            var podcastZipPath = GetPodcastZipPath(podcastId);

            if (!System.IO.File.Exists(podcastZipPath))
            {
                throw new ArgumentException("Unable to find requested zip");
            }

            var fileName = Path.GetFileName(podcastZipPath);
            return Task.FromResult(GetZipUrl(fileName));
        }

        public FileStream GetPodcastFileWriteStream(string podcastName, string fileName, string fileContentType, out string podcastId)
        {
            podcastId = GetEscapedName(podcastName);
            var finalDirectoryPath = GetPodcastDirectory(podcastId);
            var targetFileName = GetEscapedName(fileName);
            var fileFilePath = Path.Combine(finalDirectoryPath, targetFileName);
            if (!Directory.Exists(finalDirectoryPath))
            {
                Directory.CreateDirectory(finalDirectoryPath);
            }

            return new FileStream(fileFilePath, FileMode.Create);
        }

        public FileStream GetPodcastZipWriteStream(string podcastName, out string podcastId)
        {
            podcastId = GetEscapedName(podcastName);

            var path = GetPodcastZipPath(podcastId);

            return GetPodcastZipWriteStream(path);
        }

        protected FileStream GetPodcastZipWriteStream(string targetPath)
        {
            return GetPodcastZipStream(targetPath, FileMode.Create);
        }

        protected FileStream GetPodcastZipStream(string targetPath, FileMode mode)
        {
            if (!Directory.Exists(options.Value.ZippedPodcastsLocation))
            {
                Directory.CreateDirectory(options.Value.ZippedPodcastsLocation);
            }

            return new FileStream(targetPath, mode);
        }


        public Task UnzipPodcast(string podcastId)
        {
            try
            {
                var podcastZipPath = GetPodcastZipPath(podcastId);
                if (!System.IO.File.Exists(podcastZipPath))
                {
                    throw new ArgumentException("Provided zip could not be found!");
                }

                var podcastDirectory = GetPodcastDirectory(podcastId);
                if (Directory.Exists(podcastDirectory))
                {
                    Directory.Delete(podcastZipPath, true);
                }

                Directory.CreateDirectory(podcastDirectory);


                var zipStream = GetPodcastZipStream(podcastZipPath, FileMode.Open);
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                archive.ExtractToDirectory(podcastDirectory, true);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string GetPodcastDirectory(string podcastId)
        {
            var targetDirectoryName = podcastId;
            var finalDirectoryPath = Path.Combine(options.Value.PodcastsLocation, targetDirectoryName);
            return finalDirectoryPath;
        }

        private string GetPodcastZipPath(string podcastId)
        {
            var targetFileName = podcastId;
            var fileFilePath = Path.Combine(options.Value.ZippedPodcastsLocation, targetFileName);
            return $"{fileFilePath}.zip";
        }
    }
}