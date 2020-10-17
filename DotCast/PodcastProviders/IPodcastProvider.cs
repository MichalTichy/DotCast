using System;
using System.Collections.Generic;

namespace DotCast
{
    public interface IPodcastProvider
    {
        Feed GetFeed(string podcastName);
        IEnumerable<string> GetPodcastNames();
    }
}