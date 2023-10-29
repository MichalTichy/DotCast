using DotCast.Infrastructure.Persistence.Marten;
using DotCast.AudioBookProvider.Base;
using Marten;

namespace DotCast.AudioBookProvider.Postgre
{
    public class AudioBookInfoStorageConfiguration : IStorageConfiguration
    {
        public void Configure(StoreOptions options)
        {
            options.Schema.For<AudioBookInfo>().FullTextIndex(info => info.Name, info => info.AuthorName, info => info.SeriesName!);
        }
    }
}
