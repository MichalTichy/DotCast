using DotCast.SharedKernel.Models;
using Marten;
using DotCast.Infrastructure.Persistence.Specifications;

namespace DotCast.Library.Specifications
{
    internal record AudioBookStatisticSpecification : ISpecification<AudioBook, AudioBooksStatistics>
    {
        public async Task<AudioBooksStatistics?> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            var data = await queryable.ToListAsync(cancellationToken);
            var count = data.Count;
            var totalDuration = TimeSpan.FromMilliseconds(data.Sum(x => x.AudioBookInfo.Duration.TotalMilliseconds));
            var authorCount = data
                .Select(x => x.AudioBookInfo.AuthorName)
                .Where(author => !string.IsNullOrWhiteSpace(author))
                .Distinct()
                .Count();

            return new AudioBooksStatistics(count, authorCount, totalDuration);
        }
    }
}
