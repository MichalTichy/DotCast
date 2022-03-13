using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCast.Service.PodcastProviders
{
    public class CombinedPodcastProvider : IPodcastProvider
    {
        private readonly ICollection<IPodcastProvider> providers;

        public CombinedPodcastProvider(ICollection<IPodcastProvider> providers)
        {
            this.providers = providers;
        }
        public Feed GetFeed(string podcastName)
        {
            return providers.Select(t => t.GetFeed(podcastName)).SingleOrDefault(t => t != null);
        }

        public IEnumerable<PodcastInfo> GetPodcastInfo()
        {
            var podcastNames = providers.SelectMany(t=>t.GetPodcastInfo()).ToArray();
            if (podcastNames.Length != podcastNames.Distinct().Count())
            {
                throw new ArgumentException("Two podcast with the same name exist in multiple sources.");
            }

            return podcastNames.OrderBy(t => t);
        }
    }
}