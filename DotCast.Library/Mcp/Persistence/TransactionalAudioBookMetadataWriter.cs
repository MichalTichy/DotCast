using System.Data;
using DotCast.Infrastructure.CurrentTenancyProvider;
using DotCast.Library.Mcp.Models;
using DotCast.SharedKernel.Models;
using Marten;
using Npgsql;
namespace DotCast.Library.Mcp.Persistence;
public sealed class TransactionalAudioBookMetadataWriter(IDocumentStore store, ICurrentTenancyProvider tenancy) : ITransactionalAudioBookMetadataWriter
{
    public async Task<AudioBookMetadata> UpdateAsync(string id, string ownerLibrary, AudioBookMetadataPatch patch, CancellationToken cancellationToken)
    {
        patch.Validate();
        var tenantId = await tenancy.GetUserTenantAsync();
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await UpdateOnceAsync(id, ownerLibrary, patch, tenantId, cancellationToken);
            }
            catch (Exception exception) when (attempt < 3 && IsSerializationFailure(exception))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(20 * (attempt + 1)), cancellationToken);
            }
        }
    }
    private async Task<AudioBookMetadata> UpdateOnceAsync(string id, string ownerLibrary, AudioBookMetadataPatch patch, string tenantId, CancellationToken cancellationToken)
    {
        await using var session = store.LightweightSession(tenantId, IsolationLevel.Serializable);
        await session.BeginTransactionAsync(cancellationToken);
        var book = await session.LoadAsync<AudioBook>(id, cancellationToken);
        if (book is null || book.LibraryId != ownerLibrary) throw new KeyNotFoundException("not_found");
        patch.Apply(book);
        session.Update(book);
        await session.SaveChangesAsync(cancellationToken);
        return AudioBookMetadata.From(book);
    }
    private static bool IsSerializationFailure(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
            if (current is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure }) return true;
        return false;
    }
}
