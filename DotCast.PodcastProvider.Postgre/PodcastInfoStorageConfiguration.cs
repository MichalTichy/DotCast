using DotCast.Infrastructure.Persistence.Marten;
using DotCast.PodcastProvider.Base;
using Marten;

namespace DotCast.PodcastProvider.Postgre
{
    public class PodcastInfoStorageConfiguration : IStorageConfiguration
    {
        public void Configure(StoreOptions options)
        {
            options.Schema.For<PodcastInfo>();
        }
    }
}