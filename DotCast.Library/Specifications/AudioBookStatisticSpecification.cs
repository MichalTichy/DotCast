using DotCast.Infrastructure.Persistence.Base.Specifications;
using DotCast.SharedKernel.Models;
using Marten;

namespace DotCast.Library.Specifications
{
    internal record AudioBookStatisticSpecification : ISpecification<AudioBook, AudioBooksStatistics>
    {
        public async Task<AudioBooksStatistics?> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            var count = await queryable.CountAsync(cancellationToken);
            var sumAsync = await queryable.SumAsync(x => x.AudioBookInfo.Duration.TotalHours, cancellationToken);
            var totalDuration = TimeSpan.FromHours(sumAsync);
            var authorCount = await queryable.Select(x => x.AudioBookInfo.AuthorName).Distinct().CountAsync(cancellationToken);
            return new AudioBooksStatistics(count, authorCount, totalDuration);
        }
    }
}