namespace DotCast.AudioBookProvider.Base
{
    public interface IAudioBookInfoProvider
    {
        IAsyncEnumerable<AudioBookInfo> GetAudioBooks(string? searchText = null);
        Task UpdateAudioBookInfo(AudioBookInfo AudioBookInfo);
        Task<AudioBookInfo?> Get(string id);
        Task<AudioBooksStatistics> GetStatistics();
    }
}
