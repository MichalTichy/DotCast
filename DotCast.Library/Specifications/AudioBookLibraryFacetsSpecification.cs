using DotCast.Infrastructure.Persistence.Specifications;
using DotCast.SharedKernel.Models;
using Marten;

namespace DotCast.Library.Specifications
{
    internal record AudioBookLibraryFacetsSpecification : ISpecification<AudioBook, AudioBookLibraryFacets>
    {
        public async Task<AudioBookLibraryFacets?> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            var data = await queryable.ToListAsync(cancellationToken);
            var durations = data
                .Select(book => (int)Math.Ceiling(book.AudioBookInfo.Duration.TotalMinutes))
                .ToArray();

            return new AudioBookLibraryFacets(
                data.Select(book => book.AudioBookInfo.AuthorName)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim())
                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                    .OrderBy(value => value)
                    .ToArray(),
                data.SelectMany(book => book.AudioBookInfo.Categories)
                    .Select(category => category.Name)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim())
                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                    .OrderBy(value => value)
                    .ToArray(),
                data.Select(book => book.AudioBookInfo.SeriesName)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value!.Trim())
                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                    .OrderBy(value => value)
                    .ToArray(),
                durations.Length == 0 ? 0 : durations.Min(),
                durations.Length == 0 ? 0 : durations.Max());
        }
    }
}
