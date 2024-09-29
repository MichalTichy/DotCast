using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.IoC;

namespace DotCast.Infrastructure.Archival
{
    public class ArchiveManagerInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<IArchiveManager, ArchiveManager>();
        }
    }
}