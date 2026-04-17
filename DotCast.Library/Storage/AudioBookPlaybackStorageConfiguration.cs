using DotCast.Infrastructure.Persistence.Marten.StorageConfiguration;
using DotCast.SharedKernel.Models;
using Marten;

namespace DotCast.Library.Storage
{
    public class AudioBookPlaybackStorageConfiguration : IStorageConfiguration
    {
        public void Configure(StoreOptions options)
        {
            options.Schema.For<AudioBookPlayback>()
                .Identity(x => x.Id)
                .Duplicate(x => x.AudioBookId)
                .Duplicate(x => x.UserId)
                .Duplicate(x => x.Status);
        }
    }
}
