using System;
using DotCast.Infrastructure.IoC;
using DotCast.Infrastructure.Persistence.Marten.Extensions;
using DotCast.Infrastructure.Persistence.Marten.Migration;
using DotCast.Infrastructure.Persistence.Marten.StorageConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class MartenDatabaseInstaller : ILowPriorityInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var connectionString = configuration.GetConnectionString(DatabaseConstants.ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($"Missing connection string for connection {DatabaseConstants.ConnectionStringName}.");
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