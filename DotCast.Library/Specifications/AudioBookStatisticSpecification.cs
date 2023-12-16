using DotCast.Infrastructure.Persistence.Base.Specifications;
using DotCast.SharedKernel.Models;
using Marten;

namespace DotCast.Library.Specifications
{
    internal record AudioBookStatisticSpecification : ISpecification<AudioBook, AudioBooksStatistics>
    {
        public async Task<AudioBooksStatistics?> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            var statistics = await queryable
                .GroupBy(a => 1) // Group by a constant to aggregate all records
                .Select(g => new AudioBooksStatistics(
                    g.Count(), // Total count of audio books
                    g.Select(a => a.AuthorName).Distinct().Count(), // Count of distinct authors
                    TimeSpan.FromMinutes(g.Sum(a => a.Duration.TotalMinutes)) // Total duration
                ))
                .FirstOrDefaultAsync(cancellationToken);

            return statistics;
        }
    }
}