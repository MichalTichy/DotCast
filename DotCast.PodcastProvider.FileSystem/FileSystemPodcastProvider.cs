using System.IO.Compression;
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

                yield return new PodcastInfo(feed.Title, feed.AuthorName ?? "Unknown author", $"{settings.PodcastServerUrl}/podcast/{directory.Name}", feed.ImageUrl, feed.Duration);
            }
        }

        private static string GetNormalizedName(string directory)
        {
            return directory.Replace("_", " ");
        }

        private string GetFileUrl(string directoryName, string fileName)
        {
            return $"{settings.PodcastServerUrl}/files/{directoryName}/{fileName}";
        }

        public FileStream GetWriteStream(string podcastName, string fileName, string fileContentType)
        {
            var targetDirectoryName = GetNormalizedName(podcastName);
            var targetFileName = GetNormalizedName(fileName);
            var finalDirectoryPath = Path.Combine(options.Value.PodcastsLocation, targetDirectoryName);
            var fileFilePath = Path.Combine(finalDirectoryPath, targetFileName);
            if (!Directory.Exists(finalDirectoryPath))
            {
                Directory.CreateDirectory(finalDirectoryPath);
            }

            return new FileStream(fileFilePath, FileMode.Create);
        }

        private async Task<Stream> GenerateZip(string podcastName)
        {
            var targetDirectoryName = GetNormalizedName(podcastName);
            var finalDirectoryPath = Path.Combine(options.Value.PodcastsLocation, targetDirectoryName);
            if (!Directory.Exists(finalDirectoryPath))
            {
                throw new ArgumentException("Could not find requested podcast.", nameof(podcastName));
            }

            var botFilePaths = Directory.GetFiles("/path/to/bots");
            var zipFileMemoryStream = new MemoryStream();

            using (var archive = new ZipArchive(zipFileMemoryStream, ZipArchiveMode.Update, true))
            {
                foreach (var botFilePath in botFilePaths)
                {
                    var botFileName = Path.GetFileName(botFilePath);
                    var entry = archive.CreateEntry(botFileName);
                    await using var entryStream = entry.Open();
                    await using var fileStream = File.OpenRead(botFilePath);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            zipFileMemoryStream.Seek(0, SeekOrigin.Begin);
            return zipFileMemoryStream;
        }


        public string GetZipDownloadUrl(string podcastName)
        {
            throw new NotImplementedException();
        }
    }
}