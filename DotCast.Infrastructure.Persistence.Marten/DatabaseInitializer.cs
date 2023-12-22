using System.Threading.Tasks;
using Ardalis.GuardClauses;
using DotCast.Infrastructure.Initializer;
using DotCast.Infrastructure.Persistence.Marten.Migration;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Weasel.Core;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class DatabaseInitializer : InitializerBase
    {
        private readonly IDocumentStore store;
        private readonly IConfiguration configuration;
        private readonly ILogger<DatabaseInitializer> logger;
        private readonly IDatabaseMigrator databaseMigrator;

        public override int Priority => int.MaxValue;

        public DatabaseInitializer(IDocumentStore store, IConfiguration configuration, ILogger<DatabaseInitializer> logger, IDatabaseMigrator databaseMigrator)
        {
            this.store = store;
            this.configuration = configuration;
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

            var connectionString = configuration.GetConnectionString(DatabaseConstants.ConnectionStringName);
            Guard.Against.NullOrWhiteSpace(connectionString, nameof(connectionString));

            var isNewDbCreated = await CreateDatabaseIfItDoesNotExistsAsync(connectionString);

            await ApplySchemaChangesAsync();
            await ApplyMigrationsAsync(isNewDbCreated);
        }

        /// <summary>
        ///     Temporary hack while following issue is not resolved:
        ///     https://github.com/JasperFx/marten/issues/2786
        /// </summary>
        private async Task<bool> CreateDatabaseIfItDoesNotExistsAsync(string connectionString)
        {
            // Parse the connection string and replace the database name with 'postgres' temporarily
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var newDatabaseName = builder.Database;
            Guard.Against.NullOrWhiteSpace(newDatabaseName, nameof(newDatabaseName));

            builder.Database = "postgres";
            var adminConnectionString = builder.ConnectionString;

            try
            {
                await using var conn = new NpgsqlConnection(adminConnectionString);
                await conn.OpenAsync();

                // Check if the database already exists
                await using var checkDbCommand = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{newDatabaseName}'", conn);
                var databaseExists = await checkDbCommand.ExecuteScalarAsync() != null;
                if (databaseExists)
                {
                    return false;
                }

                logger.LogWarning("Database does not exists. Creating new one.");
                // Create new database
                await using var createDbCommand = new NpgsqlCommand($"CREATE DATABASE \"{newDatabaseName}\"", conn);
                await createDbCommand.ExecuteNonQueryAsync();
                return true;
            }
            catch (NpgsqlException ex)
            {
                logger.LogCritical(ex, "Error while ensuring that database exists.");
                throw;
            }
        }

        private async Task ApplySchemaChangesAsync()
        {
            logger.LogInformation("Applying database schema changes.");

            await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync(AutoCreate.CreateOrUpdate);

            await store.Options.Tenancy.Default.Database.ApplyAllConfiguredChangesToDatabaseAsync(AutoCreate.CreateOrUpdate);
        }

        private async Task ApplyMigrationsAsync(bool isNewDb)
        {
            await databaseMigrator.EnsureMigrationHistoryTableAsync();

            if (isNewDb)
            {
                logger.LogInformation("Saving the latest migration version to the history, because database was created just now and model is assumed to be up to date.");
                await databaseMigrator.SaveLastMigrationToHistoryAsync();
            }
            else
            {
                logger.LogInformation("Applying database migrations.");
                await databaseMigrator.RunMigrationsAsync();
            }
        }
    }
}