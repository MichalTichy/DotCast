using DotCast.Infrastructure.Persistence.Marten;
using DotCast.SharedKernel.Models;
using Marten;

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