using DotCast.PodcastProvider.Base;
using DotCast.RssGenerator.FromFiles;
using Microsoft.Extensions.Options;

namespace DotCast.PodcastProvider.FileSystem
{
    public class FileSystemPodcastProvider : IPodcastInfoProvider, IPodcastFeedProvider, IPodcastUploader
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
    }
}