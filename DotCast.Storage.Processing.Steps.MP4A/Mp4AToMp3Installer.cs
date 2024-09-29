using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.IoC;

namespace DotCast.Storage.Processing.Steps.MP4A
{
    public class Mp4AToMp3Installer : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<IMp4ASplitter, Mp4aSplitter>();
        }
    }
}