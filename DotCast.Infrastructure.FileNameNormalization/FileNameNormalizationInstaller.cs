using DotCast.Infrastructure.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotCast.Infrastructure.FileNameNormalization
{
    public class FileNameNormalizationInstaller: IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<IFileNameNormalizer, FileNameNormalizer>();
        }
    }
}
