namespace DotCast.PodcastProvider.Base
{
    public record PodcastInfo(string Name, string AuthorName, string Url, string? ImageUrl, TimeSpan? Duration);
}