using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotCast.Infrastructure.Initializer
{
    public class InitializerManager(ILogger<InitializerManager> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        private readonly IServiceProvider serviceProvider = serviceProvider.CreateScope().ServiceProvider;

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
