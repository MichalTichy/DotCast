using Blazorise;
using DotCast.App.Shared;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Library;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace DotCast.App.Pages
{
    [Authorize]
    public partial class AudioBooks : AppPage
    {
        private const int TypingDelay = 450;
        private const string StandaloneSeriesName = "Standalone titles";
        private const int DurationStepMinutes = 30;
        private Timer? typingTimer;

        [Inject]
        public required IJSRuntime Js { get; set; }

        [Inject]
        public required ILibraryApiInformationProvider LibraryApiInfoProvider { get; set; }

        [Inject]
        public required IMessagePublisher Messenger { get; set; }

        public IReadOnlyList<AudioBook> Data { get; set; } = [];
        public AudioBookLibraryFacets Facets { get; set; } = new([], [], []);
        public AudioBooksStatistics AudioBooksStatistics { get; set; } = new(0, 0, TimeSpan.Zero);
        public AudioBook? SelectedAudioBook { get; set; }

        private string? SearchText { get; set; }
        private string? AuthorFacetSearchText { get; set; }
        private string? CategoryFacetSearchText { get; set; }
        private HashSet<string> SelectedAuthors { get; } = new(StringComparer.InvariantCultureIgnoreCase);
        private HashSet<string> SelectedCategories { get; } = new(StringComparer.InvariantCultureIgnoreCase);
        private int? MinRating { get; set; }
        private int? MaxRating { get; set; }
        private int? MinDurationMinutes { get; set; }
        private int? MaxDurationMinutes { get; set; }
        private bool IsLoading { get; set; }
        private bool IsDeleting { get; set; }
        private string? DeleteMessage { get; set; }

        private IEnumerable<string> FilteredAuthors => FilterFacets(Facets.Authors, AuthorFacetSearchText);
        private IEnumerable<string> FilteredCategories => FilterFacets(Facets.Categories, CategoryFacetSearchText);
        private int DurationMinBound => RoundDownToStep(Facets.MinDurationMinutes, DurationStepMinutes);
        private int DurationMaxBound => Math.Max(DurationMinBound + DurationStepMinutes, RoundUpToStep(Facets.MaxDurationMinutes, DurationStepMinutes));
        private int DurationSliderMin => Clamp(MinDurationMinutes ?? DurationMinBound, DurationMinBound, DurationMaxBound - DurationStepMinutes);
        private int DurationSliderMax => Clamp(MaxDurationMinutes ?? DurationMaxBound, DurationMinBound + DurationStepMinutes, DurationMaxBound);
        private bool HasDurationBounds => Facets.MaxDurationMinutes > 0 && DurationMaxBound > DurationMinBound;
        private bool HasDurationFilter => MinDurationMinutes.HasValue || MaxDurationMinutes.HasValue;
        private string DurationFilterSummary => $"{FormatDuration(DurationSliderMin)} - {FormatDuration(DurationSliderMax)}";
        private RangeSliderValue<int> DurationRange => new(DurationSliderMin, DurationSliderMax);

        private AudioBookLibraryFilter CurrentFilter => new(
            SearchText,
            SelectedAuthors.ToArray(),
            SelectedCategories.ToArray(),
            [],
            MinRating,
            MaxRating,
            MinDurationMinutes,
            MaxDurationMinutes);

        private bool HasActiveFilters => CurrentFilter.HasActiveFilters;
        private AudioBook? FeaturedAudioBook => Data.OrderByDescending(book => book.Rating).FirstOrDefault();

        public async Task CopyRssAsync(AudioBook info)
        {
            var url = await LibraryApiInfoProvider.GetFeedUrlAsync(info.Id);
            await Js.InvokeVoidAsync("copyToClipboard", url);
        }

        public async Task Download(AudioBook info)
        {
            if (info.AudioBookInfo.HasArchive)
            {
                var url = await LibraryApiInfoProvider.GetArchiveUrlAsync(info.Id);
                await Js.InvokeVoidAsync("open", url, "_blank");
            }
        }

        public async Task DeleteAudioBook(AudioBook audioBook)
        {
            if (IsDeleting)
            {
                return;
            }

            var confirmed = await Js.InvokeAsync<bool>(
                "confirm",
                $"Permanently delete \"{audioBook.AudioBookInfo.Name}\" and all stored files? This cannot be restored from filesystem.");

            if (!confirmed)
            {
                return;
            }

            IsDeleting = true;
            DeleteMessage = null;
            await SaveStateHasChangedAsync();

            try
            {
                await Messenger.ExecuteAsync(new AudioBookDeleteRequest(audioBook.Id), PageCancellationTokenSource.Token);
                CloseDetails();
                await Task.WhenAll(LoadStatistics(), LoadFacets(), LoadData());
            }
            catch (Exception exception) when (!PageCancellationTokenSource.IsCancellationRequested)
            {
                DeleteMessage = $"Delete failed: {exception.Message}";
            }
            finally
            {
                IsDeleting = false;
                await SaveStateHasChangedAsync();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            NavigationManager.LocationChanged += OnLocationChanged;
            ReadFilterFromUrl();
            await Task.WhenAll(LoadStatistics(), LoadFacets(), LoadData());
        }

        public override async ValueTask DisposeAsync()
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
            typingTimer?.Dispose();
            await base.DisposeAsync();
        }

        private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs args)
        {
            ReadFilterFromUrl();
            _ = LoadData();
        }

        private async Task LoadStatistics()
        {
            AudioBooksStatistics = await Messenger.RequestAsync<AudioBooksStatisticsRequest, AudioBooksStatistics>(
                new AudioBooksStatisticsRequest(),
                PageCancellationTokenSource.Token);
        }

        private async Task LoadFacets()
        {
            Facets = await Messenger.RequestAsync<AudioBookLibraryFacetsRequest, AudioBookLibraryFacets>(
                new AudioBookLibraryFacetsRequest(),
                PageCancellationTokenSource.Token);
            NormalizeDurationFilter();
        }

        private async Task LoadData()
        {
            IsLoading = true;
            await SaveStateHasChangedAsync();

            Data = await Messenger.RequestAsync<AudioBooksRetrievalRequest, IReadOnlyList<AudioBook>>(
                new AudioBooksRetrievalRequest(CurrentFilter),
                PageCancellationTokenSource.Token);

            IsLoading = false;
            await SaveStateHasChangedAsync();
        }

        private void SearchTextChanged(string text)
        {
            SearchText = text;
            typingTimer?.Dispose();
            typingTimer = new Timer(_ => InvokeAsync(UpdateUrlFromFilter), null, TypingDelay, Timeout.Infinite);
        }

        private void ToggleAuthor(string author) => ToggleValue(SelectedAuthors, author);
        private void ToggleCategory(string category) => ToggleValue(SelectedCategories, category);

        private void ToggleValue(HashSet<string> target, string value)
        {
            if (!target.Add(value))
            {
                target.Remove(value);
            }

            UpdateUrlFromFilter();
        }

        private void SetRatingRange(int? minRating, int? maxRating)
        {
            MinRating = minRating;
            MaxRating = maxRating;
            UpdateUrlFromFilter();
        }

        private void SetDurationRange(RangeSliderValue<int> value)
        {
            var start = Math.Min(value.Start, value.End);
            var end = Math.Max(value.Start, value.End);

            MinDurationMinutes = start <= DurationMinBound ? null : Clamp(start, DurationMinBound, DurationMaxBound);
            MaxDurationMinutes = end >= DurationMaxBound ? null : Clamp(end, DurationMinBound, DurationMaxBound);
            UpdateUrlFromFilter();
        }

        private void ClearFilters()
        {
            SearchText = null;
            AuthorFacetSearchText = null;
            CategoryFacetSearchText = null;
            SelectedAuthors.Clear();
            SelectedCategories.Clear();
            MinRating = null;
            MaxRating = null;
            MinDurationMinutes = null;
            MaxDurationMinutes = null;
            UpdateUrlFromFilter();
        }

        private void ShowDetails(AudioBook audioBook)
        {
            SelectedAudioBook = audioBook;
            DeleteMessage = null;
        }

        private void CloseDetails()
        {
            SelectedAudioBook = null;
        }

        private void UpdateUrlFromFilter()
        {
            var query = new List<string>();
            AddQuery(query, "q", SearchText);
            AddQuery(query, "authors", SelectedAuthors);
            AddQuery(query, "categories", SelectedCategories);
            AddQuery(query, "minRating", MinRating?.ToString());
            AddQuery(query, "maxRating", MaxRating?.ToString());
            AddQuery(query, "minDuration", MinDurationMinutes?.ToString());
            AddQuery(query, "maxDuration", MaxDurationMinutes?.ToString());

            var url = query.Count == 0 ? "/" : $"/?{string.Join("&", query)}";
            NavigationManager.NavigateTo(url, replace: true);
        }

        private void ReadFilterFromUrl()
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var query = QueryHelpers.ParseQuery(uri.Query);

            SearchText = query.TryGetValue("q", out var searchText) ? searchText.ToString() : null;
            ReplaceValues(SelectedAuthors, query.TryGetValue("authors", out var authors) ? authors.ToString() : null);
            ReplaceValues(SelectedCategories, query.TryGetValue("categories", out var categories) ? categories.ToString() : null);
            MinRating = query.TryGetValue("minRating", out var minRating) && int.TryParse(minRating, out var min) ? min : null;
            MaxRating = query.TryGetValue("maxRating", out var maxRating) && int.TryParse(maxRating, out var max) ? max : null;
            MinDurationMinutes = query.TryGetValue("minDuration", out var minDuration) && int.TryParse(minDuration, out var minDurationValue) ? minDurationValue : null;
            MaxDurationMinutes = query.TryGetValue("maxDuration", out var maxDuration) && int.TryParse(maxDuration, out var maxDurationValue) ? maxDurationValue : null;

            if (Facets.MaxDurationMinutes > 0)
            {
                NormalizeDurationFilter();
            }
        }

        private void NormalizeDurationFilter()
        {
            if (!HasDurationBounds)
            {
                MinDurationMinutes = null;
                MaxDurationMinutes = null;
                return;
            }

            MinDurationMinutes = MinDurationMinutes.HasValue
                ? Clamp(MinDurationMinutes.Value, DurationMinBound, DurationMaxBound)
                : null;
            MaxDurationMinutes = MaxDurationMinutes.HasValue
                ? Clamp(MaxDurationMinutes.Value, DurationMinBound, DurationMaxBound)
                : null;

            if (MinDurationMinutes <= DurationMinBound)
            {
                MinDurationMinutes = null;
            }

            if (MaxDurationMinutes >= DurationMaxBound)
            {
                MaxDurationMinutes = null;
            }

            if (MinDurationMinutes.HasValue && MaxDurationMinutes.HasValue && MinDurationMinutes.Value >= MaxDurationMinutes.Value)
            {
                MinDurationMinutes = null;
                MaxDurationMinutes = null;
            }
        }

        private static IEnumerable<string> FilterFacets(IEnumerable<string> values, string? searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return values;
            }

            return values.Where(value => value.Contains(searchText.Trim(), StringComparison.InvariantCultureIgnoreCase));
        }

        private static void ReplaceValues(HashSet<string> target, string? value)
        {
            target.Clear();
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            foreach (var item in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                target.Add(item);
            }
        }

        private static void AddQuery(List<string> query, string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Add($"{key}={Uri.EscapeDataString(value.Trim())}");
            }
        }

        private static void AddQuery(List<string> query, string key, IEnumerable<string> values)
        {
            var value = string.Join(",", values.OrderBy(value => value));
            AddQuery(query, key, value);
        }

        private static int RoundDownToStep(int value, int step)
        {
            return value <= 0 ? 0 : value / step * step;
        }

        private static int RoundUpToStep(int value, int step)
        {
            if (value <= 0)
            {
                return step;
            }

            return (int)Math.Ceiling(value / (double)step) * step;
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        private static string FormatDuration(int minutes)
        {
            var duration = TimeSpan.FromMinutes(minutes);
            var hours = (int)duration.TotalHours;

            if (hours == 0)
            {
                return $"{duration.Minutes}m";
            }

            if (duration.Minutes == 0)
            {
                return $"{hours}h";
            }

            return $"{hours}h {duration.Minutes}m";
        }
    }
}
