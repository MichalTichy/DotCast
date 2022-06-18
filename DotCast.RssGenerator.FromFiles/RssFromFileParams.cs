namespace DotCast.RssGenerator.FromFiles
{
    public record RssFromFileParams(string PodcastName, ICollection<LocalFileInfo> Files);
}