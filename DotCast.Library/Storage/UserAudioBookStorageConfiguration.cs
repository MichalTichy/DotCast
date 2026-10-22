using DotCast.Infrastructure.Persistence.Marten.StorageConfiguration;
using DotCast.SharedKernel.Models;
using Marten;

namespace DotCast.Library.Storage
{
    public class UserAudioBookStorageConfiguration : IStorageConfiguration
    {
        public void Configure(StoreOptions options)
        {
            options.Schema.For<UserAudioBook>()
                .Identity(x => x.Id)
                .Duplicate(x => x.AudioBookId)
                .Duplicate(x => x.UserId);
        }
    }
}
