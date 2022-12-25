using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class DatabaseMigrator : IDatabaseMigrator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly INoTenancyByDefaultSessionFactory sessionFactory;
        private readonly ILogger<DatabaseMigrator> logger;

        public DatabaseMigrator(
            IServiceProvider serviceProvider,
            INoTenancyByDefaultSessionFactory sessionFactory,
            ILogger<DatabaseMigrator> logger)
        {
            this.serviceProvider = serviceProvider;
            this.sessionFactory = sessionFactory;
            this.logger = logger;
        }

        public async Task EnsureMigrationHistoryTableAsync(CancellationToken ct)
        {
            await using var session = await sessionFactory.OpenSessionAsync();
            await using var command = session.Connection.CreateCommand();

            command.CommandText = "create table if not exists __migration_history (" +
                                  "version int primary key," +
                                  "name varchar (150) not null," +
                                  "applied_on timestamp not null" +
                                  ");";

            await command.ExecuteNonQueryAsync(ct);
            await session.SaveChangesAsync(ct);
        }

        public async Task SaveLastMigrationToHistoryAsync(CancellationToken ct)
        {
            var migrations = GetOrderedMigrations();
            var lastMigration = migrations.LastOrDefault();
            if (lastMigration is null)
            {
                return;
            }

            await using var session = await sessionFactory.OpenSessionAsync();
            await AddToMigrationHistoryAsync(session, lastMigration, ct);
            await session.SaveChangesAsync(ct);
        }

        public async Task RunMigrationsAsync(CancellationToken ct)
        {
            var migrations = GetOrderedMigrations();

            var lastAppliedVersion = await GetLastAppliedVersionAsync(ct);

            var migrationsForApplication = migrations.Where(x => x.Version > lastAppliedVersion).ToList();
            LogMigrationsForApplication(migrationsForApplication, lastAppliedVersion);

            foreach (var migration in migrationsForApplication)
            {
                await ApplyMigrationAsync(migration, ct);
            }
        }

        private List<IMartenMigration> GetOrderedMigrations()
        {
            var migrations = serviceProvider.GetServices<IMartenMigration>().ToList();
            logger.LogDebug("Total count of defined migrations: {migrationsCount}", migrations.Count);

            if (migrations.DistinctBy(x => x.Version).Count() != migrations.Count)
            {
                const string message = "Found migration with duplicit version. Please adjust migrations to have distinct version numbers.";
                logger.LogCritical(message);
                throw new InvalidOperationException(message);
            }

            migrations = migrations.OrderBy(x => x.Version).ToList();
            if (migrations.Any())
            {
                logger.LogDebug("Last version of defined migrations: {migrationVersion}", migrations.Last().Version);
            }
            else
            {
                logger.LogWarning("No migrations were found.");
            }

            return migrations;
        }

        private async Task<int> GetLastAppliedVersionAsync(CancellationToken ct)
        {
            await using var session = await sessionFactory.OpenSessionAsync();
            await using var command = session.Connection.CreateCommand();

            command.CommandText = "select version from __migration_history " +
                                  "order by version desc limit 1";

            var result = await command.ExecuteScalarAsync(ct);

            if (result is not null)
            {
                logger.LogDebug("Last applied migration version: {migrationVersion}", result);
            }
            else
            {
                logger.LogDebug("No applied migrations found.");
            }

            return result is null ? int.MinValue : (int)result;
        }

        private async Task ApplyMigrationAsync(IMartenMigration migration, CancellationToken ct)
        {
            logger.LogWarning("Applying migration '{migrationName}' with version '{version}'.", migration.GetType().Name, migration.Version);

            await using var session = await sessionFactory.OpenSessionAsync();
            await migration.MigrateAsync(session, ct);
            await AddToMigrationHistoryAsync(session, migration, ct);
            await session.SaveChangesAsync(ct);
        }

        private async Task AddToMigrationHistoryAsync(IDocumentSession session, IMartenMigration migration, CancellationToken ct)
        {
            logger.LogDebug("Adding migration version {migrationVersion} to migration history.", migration.Version);

            await using var command = session.Connection.CreateCommand();

            var now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            var migrationName = migration.GetType().Name;

            command.CommandText = "insert into __migration_history (version, name, applied_on) " +
                                  $"values ({migration.Version}, '{migrationName}', '{now}');";

            await command.ExecuteNonQueryAsync(ct);
        }

        private void LogMigrationsForApplication(List<IMartenMigration> migrationsForApplication, int lastAppliedVersion)
        {
            if (migrationsForApplication.Count == 0)
            {
                logger.LogInformation("No migrations to apply, model on version '{currentVersion}' is up to date.", lastAppliedVersion);
            }
            else
            {
                logger.LogInformation("Found '{migrationsCount}' pending migrations: \r\n - {pendingMigrations}",
                    migrationsForApplication.Count,
                    string.Join(Environment.NewLine + " - ", migrationsForApplication.Select(x => x.GetType().Name)));
            }
        }
    }
}