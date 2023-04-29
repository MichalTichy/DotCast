using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Infrastructure.Persistence.Base.Specifications;
using DotCast.PodcastProvider.Base;
using Marten;

namespace DotCast.PodcastProvider.Postgre
{
    public record GetFilteredPodcasts(string? SearchedText) : IListSpecification<PodcastInfo>
    {
        public Task<IReadOnlyList<PodcastInfo>> ApplyAsync(IQueryable<PodcastInfo> queryable, CancellationToken cancellationToken = default)
        {
            if (SearchedText == null)
            {
                return queryable.ToListAsync(cancellationToken);
            }

            var normalizedSearchedText = SearchedText.ToLower();
            return queryable.Where(t => t.PlainTextSearch(normalizedSearchedText)).ToListAsync(cancellationToken);
        }
    }

    public class PostgrePodcastInfoProvider : IPodcastInfoProvider
    {
        private readonly IRepository<PodcastInfo> repository;

        public PostgrePodcastInfoProvider(IRepository<PodcastInfo> repository)
        {
            this.repository = repository;
        }

        public async IAsyncEnumerable<PodcastInfo> GetPodcasts(string? searchText = null)
        {
            searchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText;
            var spec = new GetFilteredPodcasts(searchText);
            var result = await repository.ListAsync(spec);
            foreach (var podcastInfo in result)
            {
                yield return podcastInfo;
            }
        }

        public async Task UpdatePodcastInfo(PodcastInfo podcastInfo)
        {
            await repository.StoreAsync(podcastInfo);
        }

        public async Task<PodcastInfo?> Get(string id)
        {
            return await repository.GetByIdAsync(id);
        }

        public async Task<PodcastsStatistics> GetStatistics()
        {
            var podcasts = await GetPodcasts().ToListAsync();
            return PodcastsStatistics.Create(podcasts);
        }
    }
}