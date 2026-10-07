using DotCast.Library.Playback;
using DotCast.SharedKernel.Models;
using Marten;
using DotCast.Infrastructure.Persistence.Specifications;

namespace DotCast.Library.Specifications
{
    /// <param name="UserContext">Current user's playbacks and ratings; required for listening state and personal rating filters.</param>
    internal record AudioBookRetrievalSpecification(AudioBookLibraryFilter Filter, AudioBookUserContext? UserContext = null) : IListSpecification<AudioBook>
    {
        public async Task<IReadOnlyList<AudioBook>> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            var data = await queryable.ToListAsync(cancellationToken);
            return Apply(data);
        }

        public IReadOnlyList<AudioBook> Apply(IEnumerable<AudioBook> data)
        {
            IEnumerable<AudioBook> filtered = data;
            var userContext = UserContext ?? AudioBookUserContext.Empty;

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
                filtered = filtered.Where(x => userContext.GetDisplayedRating(x).ToPercent() >= Filter.MinRating.Value);
            }

            if (Filter.MaxRating.HasValue)
            {
                filtered = filtered.Where(x => userContext.GetDisplayedRating(x).ToPercent() <= Filter.MaxRating.Value);
            }

            if (Filter.MinDurationMinutes.HasValue)
            {
                filtered = filtered.Where(x => x.AudioBookInfo.Duration.TotalMinutes >= Filter.MinDurationMinutes.Value);
            }

            if (Filter.MaxDurationMinutes.HasValue)
            {
                filtered = filtered.Where(x => x.AudioBookInfo.Duration.TotalMinutes <= Filter.MaxDurationMinutes.Value);
            }

            if (Filter.ListeningState != ListeningStateFilter.Any)
            {
                filtered = filtered.Where(x => ListeningStateResolver.Matches(userContext.GetListeningState(x.Id), Filter.ListeningState));
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
                   TextNormalizer.NormalizeForSearch(value).Contains(TextNormalizer.NormalizeForSearch(filter), StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool MatchesAny(string? value, IReadOnlyCollection<string> filters)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   filters.Any(filter => string.Equals(TextNormalizer.NormalizeForSearch(value), TextNormalizer.NormalizeForSearch(filter), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
