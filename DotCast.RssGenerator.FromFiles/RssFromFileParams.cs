namespace DotCast.RssGenerator.FromFiles
{
    public record RssFromFileParams(string AudioBookName, ICollection<LocalFileInfo> Files);
}
