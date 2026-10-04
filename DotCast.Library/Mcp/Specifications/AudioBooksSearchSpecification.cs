using DotCast.Infrastructure.Persistence.Specifications;
using DotCast.Library.Mcp.Models;
using DotCast.Library.Mcp.UseCases;
using DotCast.SharedKernel.Models;
using Marten;
namespace DotCast.Library.Mcp.Specifications;
public sealed record AudioBooksSearchSpecification(SearchAudioBooks Request, IReadOnlyCollection<string> Libraries)
    : ISpecification<AudioBook, AudioBookSearchPage>
{
    public async Task<AudioBookSearchPage?> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
    {
        var libraries = Libraries.ToArray();
        var query = queryable.Where(b => libraries.Contains(b.LibraryId));
        if (!string.IsNullOrWhiteSpace(Request.LibraryId)) query = query.Where(b => b.LibraryId == Request.LibraryId);
        if (!string.IsNullOrWhiteSpace(Request.Category)) query = query.Where(b => b.AudioBookInfo.Categories.Any(c => c.Name == Request.Category));
        if (!string.IsNullOrWhiteSpace(Request.Search))
        {
            var search = Request.Search.Trim();
            query = query.Where(b => b.AudioBookInfo.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                b.AudioBookInfo.AuthorName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (b.AudioBookInfo.SeriesName != null && b.AudioBookInfo.SeriesName.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(b => b.AudioBookInfo.AuthorName).ThenBy(b => b.AudioBookInfo.SeriesName)
            .ThenBy(b => b.AudioBookInfo.OrderInSeries).ThenBy(b => b.AudioBookInfo.Name).ThenBy(b => b.Id)
            .Skip(Request.Offset).Take(Request.Limit)
            .Select(b => new AudioBookSummary(b.Id, b.LibraryId, b.AudioBookInfo.Name, b.AudioBookInfo.AuthorName,
                b.AudioBookInfo.SeriesName, b.AudioBookInfo.OrderInSeries, b.Rating)).ToListAsync(cancellationToken);
        return new(items, total, Request.Offset, Request.Limit);
    }
}
