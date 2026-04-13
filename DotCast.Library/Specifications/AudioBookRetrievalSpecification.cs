using DotCast.SharedKernel.Models;
using Marten;
using DotCast.Infrastructure.Persistence.Specifications;
using System.Globalization;
using System.Text;

namespace DotCast.Library.Specifications
{
    internal record AudioBookRetrievalSpecification(AudioBookLibraryFilter Filter) : IListSpecification<AudioBook>
    {
        public async Task<IReadOnlyList<AudioBook>> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            var data = await queryable.ToListAsync(cancellationToken);
            IEnumerable<AudioBook> filtered = data;

            if (!string.IsNullOrWhiteSpace(Filter.SearchText))
            {
                var searchText = Filter.SearchText.Trim();
                filtered = filtered.Where(x =>
                    Contains(x.AudioBookInfo.Name, searchText) ||
                    Contains(x.AudioBookInfo.AuthorName, searchText) ||
                    Contains(x.AudioBookInfo.SeriesName, searchText) ||
                    Contains(x.AudioBookInfo.Description, searchText) ||
                    x.AudioBookInfo.Categories.Any(category => Contains(category.Name, searchText)));
            }

            if (Filter.Authors.Count > 0)
            {
                filtered = filtered.Where(x => MatchesAny(x.AudioBookInfo.AuthorName, Filter.Authors));
            }

            if (Filter.Categories.Count > 0)
            {
                filtered = filtered.Where(x => x.AudioBookInfo.Categories.Any(category => MatchesAny(category.Name, Filter.Categories)));
            }

            if (Filter.Series.Count > 0)
            {
                filtered = filtered.Where(x => MatchesAny(x.AudioBookInfo.SeriesName, Filter.Series));
            }

            if (Filter.MinRating.HasValue)
            {
                filtered = filtered.Where(x => x.Rating >= Filter.MinRating.Value);
            }

            if (Filter.MaxRating.HasValue)
            {
                filtered = filtered.Where(x => x.Rating <= Filter.MaxRating.Value);
            }

            return filtered
                .OrderBy(x => x.AudioBookInfo.AuthorName)
                .ThenBy(x => x.AudioBookInfo.SeriesName)
                .ThenBy(x => x.AudioBookInfo.OrderInSeries)
                .ThenBy(x => x.AudioBookInfo.Name)
                .ToList();
        }

        private static bool Contains(string? value, string filter)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   NormalizeForSearch(value).Contains(NormalizeForSearch(filter), StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool MatchesAny(string? value, IReadOnlyCollection<string> filters)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   filters.Any(filter => string.Equals(NormalizeForSearch(value), NormalizeForSearch(filter), StringComparison.InvariantCultureIgnoreCase));
        }

        private static string NormalizeForSearch(string value)
        {
            var normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
