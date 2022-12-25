using Marten;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public interface IStorageConfiguration
    {
        void Configure(StoreOptions options);
    }
}