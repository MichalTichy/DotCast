using DotCast.PodcastProvider.Base;

namespace DotCast.PodcastProvider.Combined
{
    public class CombinedPodcastProvider : IPodcastInfoProvider
    {
        private readonly List<IPodcastInfoProvider> providers = new();

        public void AddProvider(IPodcastInfoProvider provider)
        {
            providers.Add(provider);
        }

        public async IAsyncEnumerable<PodcastInfo> GetPodcasts(string? searchText = null)
        {
            var returnedIds = new List<string>();

            foreach (var provider in providers)
            {
                await foreach (var result in provider.GetPodcasts(searchText))
                {
                    if (returnedIds.Contains(result.Id))
                    {
                        continue;
                    }

                    returnedIds.Add(result.Id);
                    yield return result;
                }
            }
        }

        public async Task UpdatePodcastInfo(PodcastInfo podcastInfo)
        {
            var tasks = providers.Select(t => t.UpdatePodcastInfo(podcastInfo)).ToArray();

            await Task.WhenAll(tasks);
        }

        public async Task<PodcastInfo?> Get(string id)
        {
            foreach (var provider in providers)
            {
                var result = await provider.Get(id);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}