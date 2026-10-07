using DotCast.Infrastructure.IoC;
using DotCast.Infrastructure.Persistence.Marten.StorageConfiguration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace DotCast.Infrastructure.ApiKeys;
public sealed class ApiKeysInstaller : IInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.AddScoped<IApiKeyOwnerResolver, ApiKeyOwnerResolver>();
        services.AddTransient<IStorageConfiguration, AccountApiKeyStorageConfiguration>();
        services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyDefaults.Scheme, null);
    }
}
