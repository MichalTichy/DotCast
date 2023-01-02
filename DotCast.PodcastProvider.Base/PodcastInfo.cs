namespace DotCast.PodcastProvider.Base
{
    public record PodcastInfo(
        string Id,
        string Name,
        string AuthorName,
        string Url,
        string? ImageUrl,
        TimeSpan? Duration);
}