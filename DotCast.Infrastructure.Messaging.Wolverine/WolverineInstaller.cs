using DotCast.Infrastructure.Messaging.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.IoC;

namespace DotCast.Infrastructure.Messaging.Wolverine
{
    public class WolverineInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddTransient<IMessagePublisher, WolverineMessagePublisher>();
        }
    }
}