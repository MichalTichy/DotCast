namespace DotCast
{
    public interface IEpisodeProvider
    {
        Feed GetFeed(string podcastName);
    }
}