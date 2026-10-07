using DotCast.Infrastructure.Persistence.Marten.StorageConfiguration;
using Marten;
namespace DotCast.Infrastructure.ApiKeys;
public sealed class AccountApiKeyStorageConfiguration : IStorageConfiguration
{
    public void Configure(StoreOptions options)
    {
        var mapping = options.Schema.For<AccountApiKey>().SingleTenanted().Identity(k => k.Id);
        mapping.Index(k => k.Hash);
    }
}
