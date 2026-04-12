using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DotCast.Infrastructure.IoC;

namespace DotCast.Infrastructure.UnitOfWork;

public class UnitOfWorkExecutorInstaller : ILowPriorityInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.TryAddSingleton(typeof(IUnitOfWorkExecutor), typeof(NoUnitOfWorkExecutor));
    }
}
