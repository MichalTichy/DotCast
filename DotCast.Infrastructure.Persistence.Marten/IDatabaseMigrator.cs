using System.Threading;
using System.Threading.Tasks;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public interface IDatabaseMigrator
    {
        Task EnsureMigrationHistoryTableAsync(CancellationToken ct = default);
        Task SaveLastMigrationToHistoryAsync(CancellationToken ct = default);
        Task RunMigrationsAsync(CancellationToken ct = default);
    }
}
