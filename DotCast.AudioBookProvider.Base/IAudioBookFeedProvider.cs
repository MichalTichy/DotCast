namespace DotCast.AudioBookProvider.Base
{
    public interface IAudioBookFeedProvider
    {
        Task<string> GetRss(string id);
    }
}
