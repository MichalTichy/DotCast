namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastFeedProvider
    {
        Task<string> GetRss(string name);
    }
}