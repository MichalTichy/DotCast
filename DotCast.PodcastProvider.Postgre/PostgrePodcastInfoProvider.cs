using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.PodcastProvider.Base;

namespace DotCast.PodcastProvider.Postgre
{
    public class PostgrePodcastInfoProvider : IPodcastInfoProvider
    {
        private readonly object repository;

        public PostgrePodcastInfoProvider(IRepository<PodcastInfo> repository)
        {
            this.repository = repository;
        }

        public IAsyncEnumerable<PodcastInfo> GetPodcasts(string? searchText = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePodcastInfo(PodcastInfo podcastInfo)
        {
            throw new NotImplementedException();
        }

        public Task<PodcastInfo> Get(string id)
        {
            throw new NotImplementedException();
        }
    }
}