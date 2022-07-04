namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastUploader
    {
        FileStream GetPodcastWriteStream(string podcastName, string fileName, string fileContentType, out string podcastId);
    }
}