namespace DotCast.AudioBookProvider.Base
{
    public record AudioBookInfo(
        string Id,
        string Name,
        string AuthorName,
        string? SeriesName,
        int OrderInSeries,
        string? Description,
        string Url,
        string? ImageUrl,
        int Rating,
        TimeSpan? Duration);
}
