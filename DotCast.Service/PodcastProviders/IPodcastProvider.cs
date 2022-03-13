using System.Collections.Generic;

namespace DotCast.Service.PodcastProviders
{
    public interface IPodcastProvider
    {
        Feed GetFeed(string podcastName);
        IEnumerable<PodcastInfo> GetPodcastInfo();
    }
}