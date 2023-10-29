using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Infrastructure.Persistence.Base.Specifications;
using DotCast.AudioBookProvider.Base;
using Marten;

namespace DotCast.AudioBookProvider.Postgre
{
    public record GetFilteredAudioBooks(string? SearchedText) : IListSpecification<AudioBookInfo>
    {
        public Task<IReadOnlyList<AudioBookInfo>> ApplyAsync(IQueryable<AudioBookInfo> queryable, CancellationToken cancellationToken = default)
        {
            if (SearchedText == null)
            {
                return queryable.ToListAsync(cancellationToken);
            }

            var normalizedSearchedText = SearchedText.ToLower();
            return queryable.Where(t => t.PlainTextSearch(normalizedSearchedText)).ToListAsync(cancellationToken);
        }
    }

    public class PostgreAudioBookInfoProvider : IAudioBookInfoProvider
    {
        private readonly IRepository<AudioBookInfo> repository;

        public PostgreAudioBookInfoProvider(IRepository<AudioBookInfo> repository)
        {
            this.repository = repository;
        }

        public async IAsyncEnumerable<AudioBookInfo> GetAudioBooks(string? searchText = null)
        {
            searchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText;
            var spec = new GetFilteredAudioBooks(searchText);
            var result = await repository.ListAsync(spec);
            foreach (var AudioBookInfo in result)
            {
                yield return AudioBookInfo;
            }
        }

        public async Task UpdateAudioBookInfo(AudioBookInfo AudioBookInfo)
        {
            await repository.StoreAsync(AudioBookInfo);
        }

        public async Task<AudioBookInfo?> Get(string id)
        {
            return await repository.GetByIdAsync(id);
        }

        public async Task<AudioBooksStatistics> GetStatistics()
        {
            var AudioBooks = await GetAudioBooks().ToListAsync();
            return AudioBooksStatistics.Create(AudioBooks);
        }
    }
}
