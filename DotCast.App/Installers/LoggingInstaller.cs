using Shared.Infrastructure.IoC;

namespace DotCast.App.Installers
{
    public class LoggingInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddLogging(loggingBuilder =>
            {
                var seq = configuration["Seq:Url"];

                if (!string.IsNullOrWhiteSpace(seq))
                {
                    loggingBuilder.AddSeq(seq, configuration["Seq:ApiKey"]);
                }

                loggingBuilder.AddConsole();
                loggingBuilder.AddSystemdConsole();
            });
        }
    }
}
