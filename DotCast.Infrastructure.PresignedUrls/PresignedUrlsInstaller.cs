using DotCast.Infrastructure.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotCast.Infrastructure.PresignedUrls
{
    public class PresignedUrlsInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<IPresignedUrlManager, PresignedUrlManager>();
        }
    }
}
