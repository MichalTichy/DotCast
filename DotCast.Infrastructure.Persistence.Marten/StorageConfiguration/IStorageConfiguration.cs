using Marten;

namespace DotCast.Infrastructure.Persistence.Marten.StorageConfiguration
{
    public interface IStorageConfiguration
    {
        void Configure(StoreOptions options);
    }
}
