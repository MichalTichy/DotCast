namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastFeedProvider
    {
        Task<string> GetRss(string id);
        Task<string?> GetFeedCover(string id);
    }
}