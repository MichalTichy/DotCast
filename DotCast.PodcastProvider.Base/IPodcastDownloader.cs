namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastDownloader
    {
        string GetZipDownloadUrl(string podcastName);
    }
}