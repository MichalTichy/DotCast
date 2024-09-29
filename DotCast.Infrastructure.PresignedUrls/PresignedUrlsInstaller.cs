using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.IoC;

namespace DotCast.Infrastructure.PresignedUrls
{
    public class PresignedUrlsInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<IPresignedUrlManager, PresignedUrlManager>();
            services.Configure<PresignedUrlOptions>(configuration.GetSection(nameof(PresignedUrlOptions)));
        }
    }
}
