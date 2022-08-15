﻿using DotCast.PodcastProvider.Base;
using Microsoft.Extensions.Caching.Memory;

namespace DotCast.PodcastProvider.CachedProvider
{
    public class CachedPodcastProvider<TProvider> : IPodcastFeedProvider, IPodcastInfoProvider where TProvider : IPodcastFeedProvider, IPodcastInfoProvider
    {
        private TProvider provider;
        private readonly IMemoryCache cache;

        public CachedPodcastProvider(TProvider provider, IMemoryCache cache)
        {
            this.provider = provider;
            this.cache = cache;
        }

        public string GetRss(string name)
        {
            return cache.GetOrCreate($"rss-{name}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return provider.GetRss(name);
            });
        }

        public IEnumerable<PodcastInfo> GetPodcasts(string? searchText = null)
        {
            return cache.GetOrCreate<ICollection<PodcastInfo>>($"podcasts-{searchText ?? string.Empty}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return provider.GetPodcasts(searchText).ToArray();
            });
        }

        public IEnumerable<string> GetPodcastIdsAvailableForDownload()
        {
            return cache.GetOrCreate<ICollection<string>>("podcastZips", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return provider.GetPodcastIdsAvailableForDownload().ToArray();
            });
        }
    }
}