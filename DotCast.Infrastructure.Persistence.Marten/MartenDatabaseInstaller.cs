using System;
using DotCast.Infrastructure.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class MartenDatabaseInstaller : ILowPriorityInstaller
    {
        public const string ConnectionStringName = "DotCast";

        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("No connection string found!");
            }

            var storageConfigurations = services.BuildServiceProvider().GetServices<IStorageConfiguration>();

            services.AddMartenPostgresPersistence(connectionString, options =>
            {
                foreach (var storageConfiguration in storageConfigurations)
                {
                    storageConfiguration.Configure(options);
                }
            });

            services.AddTransient<IDatabaseMigrator, DatabaseMigrator>();
        }
    }
}
