using DotCast.Infrastructure.IoC;
using DotCast.Infrastructure.Persistence.Marten.StorageConfiguration;
using DotCast.Infrastructure.PersonalApiTokens.Authentication;
using DotCast.Infrastructure.PersonalApiTokens.Persistence;
using DotCast.Infrastructure.PersonalApiTokens.UseCases;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace DotCast.Infrastructure.PersonalApiTokens;
public sealed class PersonalApiTokensInstaller : IInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IPersonalApiTokenStore, PersonalApiTokenStore>();
        services.AddScoped<ITokenOwnerResolver, TokenOwnerResolver>();
        services.AddScoped<IPersonalTokenContext, PersonalTokenContext>();
        services.AddScoped<TokenManagementAccess>();
        services.AddTransient<IStorageConfiguration, PersonalApiTokenStorageConfiguration>();
        services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, PersonalTokenAuthenticationHandler>(PersonalTokenDefaults.Scheme, null);
    }
}
