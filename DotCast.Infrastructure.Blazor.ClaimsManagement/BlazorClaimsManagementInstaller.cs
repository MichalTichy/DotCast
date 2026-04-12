using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotCast.Infrastructure.IoC;

namespace DotCast.Infrastructure.Blazor.ClaimsManagement;

public class BlazorClaimsManagementInstaller : IInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.AddScoped<IUserClaimsProvider, UserClaimsProvider>();
        services.AddScoped<IHttpContextUserInfoSetter, HttpContextUserInfoSetter>();
    }
}
