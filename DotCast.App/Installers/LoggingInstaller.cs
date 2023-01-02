using DotCast.Infrastructure.IoC;

namespace DotCast.App.Installers
{
    public class LoggingInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddSystemdConsole();
            });
        }
    }
}