using System;
using System.Threading.Tasks;
using DotCast.Infrastructure.Initializer;
using Marten;
using Microsoft.Extensions.Logging;
using Weasel.Core;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class DatabaseInitializer : InitializerBase
    {
        private readonly IDocumentStore store;
        private readonly ILogger<DatabaseInitializer> logger;
        private readonly IDatabaseMigrator databaseMigrator;

        public override int Priority => int.MaxValue;

        public DatabaseInitializer(IDocumentStore store, ILogger<DatabaseInitializer> logger, IDatabaseMigrator databaseMigrator)
        {
            this.store = store;
            this.logger = logger;
            this.databaseMigrator = databaseMigrator;
        }

        protected override async Task RunInitializationLogicAsync()
        {
            var documentTypes = store.Options.AllKnownDocumentTypes();
            foreach (var documentType in documentTypes)
            {
                logger.LogInformation("Found document {name}", documentType.Alias);
            }

            var isDatabaseCreated = await IsDatabaseCreatedAsync();

            if (!isDatabaseCreated)
            {
                logger.LogWarning("Database does not exist and will be created.");
            }

            await ApplySchemaChangesAsync();
            await ApplyMigrationsAsync(isDatabaseCreated);
        }

        private async Task<bool> IsDatabaseCreatedAsync()
        {
            await using var session = store.OpenSession();
            await using var command = session.Connection.CreateCommand();
            command.CommandText = "select count(*) from information_schema.tables where table_schema = 'public';";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        private async Task ApplySchemaChangesAsync()
        {
            logger.LogInformation("Applying database schema changes.");

            await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync(AutoCreate.CreateOrUpdate);
        }

        private async Task ApplyMigrationsAsync(bool isDatabaseCreated)
        {
            await databaseMigrator.EnsureMigrationHistoryTableAsync();

            if (isDatabaseCreated)
            {
                logger.LogInformation("Applying database migrations.");
                await databaseMigrator.RunMigrationsAsync();
            }
            else
            {
                logger.LogInformation("Saving the latest migration version to the history, because database was created just now and model is assumed to be up to date.");
                await databaseMigrator.SaveLastMigrationToHistoryAsync();
            }
        }
    }
}