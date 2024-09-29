using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.Infrastructure.BookInfoProvider.DatabazeKnih;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.IoC;

namespace DotCast.BookInfoProvider
{
    public class AudiobookInfoProviderInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton<IBookInfoProvider, DatabazeKnihBookInfoProvider>();
        }
    }
}