using DotCast.SharedKernel.Models;
using Marten;
using Shared.Infrastructure.Persistence.Marten.StorageConfiguration;

namespace DotCast.Library.Storage
{
    public class AudioBookStorageConfiguration : IStorageConfiguration
    {
        public void Configure(StoreOptions options)
        {
            options.Schema.For<AudioBook>().Identity(x => x.Id);
        }
    }
}