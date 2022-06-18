namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastFeedProvider
    {
        string GetRss(string name);
    }
}