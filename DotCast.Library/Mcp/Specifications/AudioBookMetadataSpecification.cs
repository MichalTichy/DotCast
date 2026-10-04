using DotCast.Infrastructure.Persistence.Specifications;
using DotCast.Library.Mcp.Models;
using DotCast.SharedKernel.Models;
using Marten;
namespace DotCast.Library.Mcp.Specifications;
public sealed record AudioBookMetadataSpecification(string Id, IReadOnlyCollection<string> Libraries) : ISpecification<AudioBook, AudioBookMetadata>
{
    public async Task<AudioBookMetadata?> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
    {
        var libraries = Libraries.ToArray();
        var book = await queryable.Where(b => b.Id == Id && libraries.Contains(b.LibraryId)).FirstOrDefaultAsync(cancellationToken);
        return book is null ? null : AudioBookMetadata.From(book);
    }
}
