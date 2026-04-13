namespace DotCast.SharedKernel.Models
{
    public record AudioBookLibraryFilter(
        string? SearchText = null,
        IReadOnlyCollection<string>? Authors = null,
        IReadOnlyCollection<string>? Categories = null,
        IReadOnlyCollection<string>? Series = null,
        int? MinRating = null,
        int? MaxRating = null)
    {
        public static AudioBookLibraryFilter Empty { get; } = new();

        public IReadOnlyCollection<string> Authors { get; init; } = Normalize(Authors);
        public IReadOnlyCollection<string> Categories { get; init; } = Normalize(Categories);
        public IReadOnlyCollection<string> Series { get; init; } = Normalize(Series);

        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(SearchText) ||
            Authors.Count > 0 ||
            Categories.Count > 0 ||
            Series.Count > 0 ||
            MinRating.HasValue ||
            MaxRating.HasValue;

        private static IReadOnlyCollection<string> Normalize(IReadOnlyCollection<string>? values)
        {
            return values?
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .OrderBy(value => value)
                .ToArray() ?? [];
        }
    }
}
