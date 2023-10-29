using DotCast.AudioBookInfo;
using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.AudioBookProvider.Base;

namespace DotCast.AudioBookProvider.Postgre
{
    public class PostgreAudioBookInfoProvider : IAudioBookInfoProvider
    {
        private readonly IRepository<AudioBook> repository;

        public PostgreAudioBookInfoProvider(IRepository<AudioBook> repository)
        {
            this.repository = repository;
        }

        public async IAsyncEnumerable<AudioBook> GetAudioBooks(string? searchText = null)
        {
            searchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText;
            var spec = new GetFilteredAudioBooks(searchText);
            var result = await repository.ListAsync(spec);
            foreach (var audioBook in result)
            {
                yield return audioBook;
            }
        }

        public async Task UpdateAudioBook(AudioBook audioBook)
        {
            await repository.StoreAsync(audioBook);
        }

        public async Task<AudioBook?> Get(string id)
        {
            return await repository.GetByIdAsync(id);
        }

        public async Task<AudioBooksStatistics> GetStatistics()
        {
            var audioBooks = await GetAudioBooks().ToListAsync();
            return AudioBooksStatistics.Create(audioBooks);
        }
    }
}
