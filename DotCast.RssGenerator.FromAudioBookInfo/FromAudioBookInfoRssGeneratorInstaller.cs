using DotCast.Infrastructure.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotCast.RssGenerator.FromAudioBookInfo
{
    public class FromAudioBookInfoRssGeneratorInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<FromAudioBookInfoRssGenerator>();
        }
    }
}