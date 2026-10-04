using DotCast.Library.Mcp.Models;
namespace DotCast.Library.Mcp.Persistence;
public interface ITransactionalAudioBookMetadataWriter
{
    Task<AudioBookMetadata> UpdateAsync(string id, string ownerLibrary, AudioBookMetadataPatch patch, CancellationToken cancellationToken);
}
