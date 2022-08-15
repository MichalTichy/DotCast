namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastInfoProvider
    {
        IEnumerable<PodcastInfo> GetPodcasts(string? searchText = null);
        IEnumerable<string> GetPodcastIdsAvailableForDownload();
    }
}