namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastInfoProvider
    {
        IAsyncEnumerable<PodcastInfo> GetPodcasts(string? searchText = null);
        Task UpdatePodcastInfo(PodcastInfo podcastInfo);
        Task<PodcastInfo?> Get(string id);
    }
}