using Marten;

namespace DotCast.Infrastructure.Persistence.Marten.Migration;

public interface IMartenMigration
{
    int Version { get; }
    Task MigrateAsync(IDocumentSession session, CancellationToken ct);
}
