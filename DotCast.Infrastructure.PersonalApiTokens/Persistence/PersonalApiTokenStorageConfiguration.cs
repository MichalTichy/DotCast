using DotCast.Infrastructure.Persistence.Marten.StorageConfiguration;
using DotCast.Infrastructure.PersonalApiTokens.Models;
using Marten;
namespace DotCast.Infrastructure.PersonalApiTokens.Persistence;
public sealed class PersonalApiTokenStorageConfiguration : IStorageConfiguration
{
    public void Configure(StoreOptions options) => options.Schema.For<PersonalApiToken>().SingleTenanted().Identity(t => t.Id);
}
