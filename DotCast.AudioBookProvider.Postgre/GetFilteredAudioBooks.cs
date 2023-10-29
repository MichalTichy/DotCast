using DotCast.AudioBookInfo;
using DotCast.AudioBookProvider.Base;
using DotCast.Infrastructure.Persistence.Base.Specifications;
using Marten;

namespace DotCast.AudioBookProvider.Postgre
{
    public record GetFilteredAudioBooks(string? SearchedText) : IListSpecification<AudioBook>
    {
        public Task<IReadOnlyList<AudioBook>> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            if (SearchedText == null)
            {
                return queryable.ToListAsync(cancellationToken);
            }

            var normalizedSearchedText = SearchedText.ToLower();
            return queryable.Where(t => t.PlainTextSearch(normalizedSearchedText)).ToListAsync(cancellationToken);
        }
    }
}