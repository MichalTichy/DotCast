using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotCast.Infrastructure.Initializer
{
    public class InitializerManager
    {
        private readonly ILogger<InitializerManager> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;

        public InitializerManager(ILogger<InitializerManager> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            this.configuration = configuration;
        }

        public async Task RunAllInitializersAsync()
        {
            var initializers = serviceProvider.GetServices<IInitializer>().ToArray();
            logger.LogInformation("Running {initializerCount} initializers.", initializers.Count());
            foreach (var initializer in initializers.OrderByDescending(t => t.Priority))
            {
                logger.LogInformation("Running initializer {initializerName} - priority {priority}.", initializer.Name, initializer.Priority);

                await initializer.InitializeAsync(configuration);

                logger.LogInformation("Initializer {initializerName} finished.", initializer.Name);
            }
        }
    }
}
