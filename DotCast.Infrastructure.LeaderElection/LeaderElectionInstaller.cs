using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DotCast.Infrastructure.IoC;

namespace DotCast.Infrastructure.LeaderElection;

public class LeaderElectionInstaller : ILowPriorityInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.TryAddSingleton(typeof(ILeaderElection), typeof(LeaderElectionSingleInstance));
    }
}
