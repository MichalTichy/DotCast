namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastUploader
    {
        FileStream GetWriteStream(string podcastName, string fileName, string fileContentType);
    }
}