using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DotCast.Infrastructure.IoC;

namespace DotCast.Infrastructure.CurrentTenancyProvider;

public class CurrentTenancyProviderInstaller : ILowPriorityInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.TryAddSingleton<ICurrentTenancyProvider,CurrentTenancyProviderNoTenancy>();
        services.TryAddSingleton<ITenancySwitcher,TenancySwitcher>();
    }
}
