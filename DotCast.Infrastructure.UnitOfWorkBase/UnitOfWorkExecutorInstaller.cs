using DotCast.Infrastructure.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotCast.Infrastructure.UnitOfWorkBase;

public class UnitOfWorkExecutorInstaller : ILowPriorityInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.TryAddSingleton(typeof(IUnitOfWorkExecutor), typeof(NoUnitOfWorkExecutor));
    }
}
