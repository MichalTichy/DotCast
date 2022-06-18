namespace DotCast.PodcastProvider.Base
{
    public interface IPodcastInfoProvider
    {
        IEnumerable<PodcastInfo> GetPodcasts();
    }
}