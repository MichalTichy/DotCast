namespace DotCast.PodcastProvider.Base
{
    public record PodcastInfo(
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