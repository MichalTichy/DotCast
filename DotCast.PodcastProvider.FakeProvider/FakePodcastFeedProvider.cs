using DotCast.PodcastProvider.Base;

namespace DotCast.PodcastProvider.FakeProvider
{
    public class FakePodcastFeedProvider : IPodcastFeedProvider
    {
        public string GetRss(string name)
        {
            return "<?xml version=\"1.0\" encoding=\"UTF - 8\"?>";
        }
    }
}