using DotCast.Infrastructure.Persistence.Marten.StorageConfiguration;
using Marten;
using Marten.Services;
using Newtonsoft.Json.Serialization;
namespace DotCast.Infrastructure.ApiKeys;
public sealed class AccountApiKeyStorageConfiguration : IStorageConfiguration
{
    public void Configure(StoreOptions options)
    {
        var mapping = options.Schema.For<AccountApiKey>().SingleTenanted().Identity(k => k.Id).DocumentAlias("accountapikey");
        mapping.Index(k => k.Hash);
        if (options.Serializer() is JsonNetSerializer serializer)
            serializer.Configure(settings => settings.SerializationBinder = new LegacyKeyTypeBinder(
                settings.SerializationBinder ?? new DefaultSerializationBinder()));
    }
    // Previously stored documents include this CLR type name in their JSON.
    private sealed class LegacyKeyTypeBinder(ISerializationBinder fallback) : ISerializationBinder
    {
        public Type BindToType(string? assemblyName, string typeName) =>
            assemblyName == "DotCast.Library.Mcp" && typeName == "DotCast.Library.Mcp.ApiKeys.AccountApiKey"
                ? typeof(AccountApiKey) : fallback.BindToType(assemblyName, typeName);
        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName) =>
            fallback.BindToName(serializedType, out assemblyName, out typeName);
    }
}
