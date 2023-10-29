using DotCast.AudioBookProvider.Base;

namespace DotCast.AudioBookProvider.Combined
{
    public class CombinedAudioBookProvider : IAudioBookInfoProvider
    {
        private readonly List<IAudioBookInfoProvider> providers = new();

        public async IAsyncEnumerable<AudioBookInfo> GetAudioBooks(string? searchText = null)
        {
            var returnedIds = new List<string>();

            foreach (var provider in providers)
            {
                await foreach (var result in provider.GetAudioBooks(searchText))
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

        public async Task UpdateAudioBookInfo(AudioBookInfo AudioBookInfo)
        {
            var tasks = providers.Select(t => t.UpdateAudioBookInfo(AudioBookInfo)).ToArray();

            await Task.WhenAll(tasks);
        }

        public async Task<AudioBookInfo?> Get(string id)
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

        public async Task<AudioBooksStatistics> GetStatistics()
        {
            var tasks = providers.Select(t => t.GetStatistics()).ToArray();
            await Task.WhenAll(tasks);

            var max = 0;
            AudioBooksStatistics finalResult = null!;
            foreach (var task in tasks)
            {
                var result = await task;
                if (result.TotalCount <= max)
                {
                    continue;
                }

                max = result.TotalCount;
                finalResult = result;
            }

            return finalResult;
        }

        public void AddProvider(IAudioBookInfoProvider provider)
        {
            providers.Add(provider);
        }
    }
}
