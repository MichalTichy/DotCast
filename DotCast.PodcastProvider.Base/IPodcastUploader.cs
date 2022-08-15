namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastUploader
    {
        FileStream GetPodcastFileWriteStream(string podcastName, string fileName, string fileContentType, out string podcastId);
        FileStream GetPodcastZipWriteStream(string podcastName, out string podcastId);
        Task UnzipPodcast(string podcastId);
    }
}