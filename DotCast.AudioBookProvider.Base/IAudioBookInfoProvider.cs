using DotCast.AudioBookInfo;

namespace DotCast.AudioBookProvider.Base
{
    public interface IAudioBookInfoProvider
    {
        IAsyncEnumerable<AudioBook> GetAudioBooks(string? searchText = null);
        Task UpdateAudioBook(AudioBook audioBook);
        Task<AudioBook?> Get(string id);
        Task<AudioBooksStatistics> GetStatistics();
    }
}
