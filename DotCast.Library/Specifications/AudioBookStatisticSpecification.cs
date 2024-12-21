using DotCast.SharedKernel.Models;
using Marten;
using Shared.Infrastructure.Persistence.Specifications;

namespace DotCast.Library.Specifications
{
    internal record AudioBookStatisticSpecification : ISpecification<AudioBook, AudioBooksStatistics>
    {
        public async Task<AudioBooksStatistics?> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            var data = await queryable.Select(t => new { Author = t.AudioBookInfo.AuthorName, t.AudioBookInfo.Duration }).ToListAsync(cancellationToken);
            var count = data.Count();
            var totalHours = data.Sum(x => x.Duration.TotalHours);
            var totalDuration = TimeSpan.FromHours(totalHours);
            var authorCount = data.Select(x => x.Author).Distinct().Count();
            return new AudioBooksStatistics(count, authorCount, totalDuration);
        }
    }
}