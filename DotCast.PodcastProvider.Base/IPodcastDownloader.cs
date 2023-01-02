namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastDownloader
    {
        IEnumerable<string> GetPodcastIdsAvailableForDownload();

        bool IsDownloadSupported(string podcastId);
        Task<string> GetZipDownloadUrl(string podcastId);
        Task GenerateZip(string podcastId, bool replace = false);
    }
}