using DotCast.PodcastProvider.Base;

namespace DotCast.PodcastProvider.FakeProvider
{
    public class FakePodcastFeedProvider : IPodcastFeedProvider
    {
        public Task<string> GetRss(string name)
        {
            var result = "<?xml version=\"1.0\" encoding=\"UTF - 8\"?>";
            return Task.FromResult(result);
        }
    }
}