using DotCast.SharedKernel.Models;
namespace DotCast.Library.Mcp.Models;
public sealed record AudioBookMetadata(string Id, string LibraryId, string Title, string Author,
    string? Description, string? Series, int PositionInSeries, IReadOnlyList<string> Categories,
    DateTime? ReleaseDate, string? ImageUrl, int Rating, IReadOnlyList<ChapterSummary> Chapters)
{
    public IReadOnlyList<string> AllowedCategories => Category.GetAll().Select(c => c.Name).ToArray();
    public static AudioBookMetadata From(AudioBook book) => new(book.Id, book.LibraryId,
        book.AudioBookInfo.Name, book.AudioBookInfo.AuthorName, book.AudioBookInfo.Description,
        book.AudioBookInfo.SeriesName, book.AudioBookInfo.OrderInSeries,
        book.AudioBookInfo.Categories.Select(c => c.Name).ToArray(), book.AudioBookInfo.ReleaseDate,
        book.AudioBookInfo.ImageUrl, book.Rating,
        book.AudioBookInfo.Chapters.Select(c => new ChapterSummary(c.Name, c.DurationInMinutes)).ToArray());
}
