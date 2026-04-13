using Blazorise;
using DotCast.App.Shared;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace DotCast.App.Pages
{
    [Authorize]
    public partial class AudioBookEdit : AppPage
    {
        [Inject]
        public required IMessagePublisher Messenger { get; set; }

        [Parameter]
        public required string Id { get; set; }

        public AudioBook Data { get; set; } = new()
        {
            Id = "TMP",
            AudioBookInfo = new AudioBookInfo { Id = "TMP", Name = "LOADING", AuthorName = "", Chapters = [] },
            LibraryId = "TMP"
        };

        public IReadOnlyCollection<FoundBookInfo> Suggestions { get; set; } = [];
        public ICollection<Category> MissingCategories { get; set; } = [];
        public FoundBookInfo? SelectedSuggestion { get; set; }
        public string ActiveTab { get; set; } = "metadata";
        public string? SaveMessage { get; set; }
        public bool IsLoadingSuggestions { get; set; }
        public bool SuggestionsLoaded { get; set; }
        public string? SuggestionsStatusMessage { get; set; }

        public bool ApplyTitle { get; set; } = true;
        public bool ApplyAuthor { get; set; } = true;
        public bool ApplySeries { get; set; } = true;
        public bool ApplyDescription { get; set; } = true;
        public bool ApplyCategories { get; set; } = true;
        public bool ApplyRating { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            UpdateMissingCategories();
            await base.OnInitializedAsync();
        }

        private async Task LoadData()
        {
            var request = new AudioBookDetailRequest(Id);
            var response = await Messenger.RequestAsync<AudioBookDetailRequest, AudioBook>(request, PageCancellationTokenSource.Token);

            Data = response ?? throw new ArgumentException("Requested AudioBook not found");
        }

        public void UpdateMissingCategories()
        {
            var allCategories = Category.GetAll().ToList();
            MissingCategories = allCategories.Except(Data.AudioBookInfo.Categories).ToList();
        }

        public async Task SaveAndExit()
        {
            await Save();
            NavigationManager.NavigateTo("/");
        }

        public async Task Save()
        {
            await Messenger.ExecuteAsync(new AudioBookEdited(Data));
            SaveMessage = "Saved";
        }

        private async Task LoadSuggestions()
        {
            IsLoadingSuggestions = true;
            SuggestionsLoaded = false;
            SuggestionsStatusMessage = null;
            Suggestions = [];
            SelectedSuggestion = null;
            await InvokeAsync(StateHasChanged);

            try
            {
                var request = new AudiobookInfoSuggestionsRequest(Data.AudioBookInfo.Name);
                var response = await Messenger.RequestAsync<AudiobookInfoSuggestionsRequest, IReadOnlyCollection<FoundBookInfo>>(request, PageCancellationTokenSource.Token);
                Suggestions = response.Where(IsValidSuggestion).ToList();
                SelectedSuggestion = Suggestions.FirstOrDefault();
            }
            catch (Exception exception) when (!PageCancellationTokenSource.IsCancellationRequested)
            {
                SuggestionsStatusMessage = $"Suggestions could not be loaded: {exception.Message}";
            }
            finally
            {
                SuggestionsLoaded = true;
                IsLoadingSuggestions = false;
            }
        }

        private static bool IsValidSuggestion(FoundBookInfo suggestion)
        {
            return !string.IsNullOrWhiteSpace(suggestion.Title)
                   && !string.Equals(suggestion.Title, "ERROR", StringComparison.OrdinalIgnoreCase)
                   && !string.IsNullOrWhiteSpace(suggestion.Author)
                   && !string.Equals(suggestion.Author, "ERROR", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ShowSuggestions()
        {
            await ShowSuggestions(false);
        }

        private async Task RefreshSuggestions()
        {
            await ShowSuggestions(true);
        }

        private async Task ShowSuggestions(bool force)
        {
            ActiveTab = "suggestions";

            if (IsLoadingSuggestions || SuggestionsLoaded && !force)
            {
                return;
            }

            await LoadSuggestions();
        }

        private void SelectSuggestion(FoundBookInfo suggestion)
        {
            SelectedSuggestion = suggestion;
        }

        private void ApplySelectedSuggestion()
        {
            if (SelectedSuggestion is null)
            {
                return;
            }

            if (ApplyTitle)
            {
                Data.AudioBookInfo.Name = SelectedSuggestion.Title;
            }

            if (ApplyAuthor)
            {
                Data.AudioBookInfo.AuthorName = SelectedSuggestion.Author;
            }

            if (ApplySeries)
            {
                Data.AudioBookInfo.SeriesName = SelectedSuggestion.SeriesName;
                Data.AudioBookInfo.OrderInSeries = SelectedSuggestion.OrderInSeries;
            }

            if (ApplyDescription)
            {
                Data.AudioBookInfo.Description = SelectedSuggestion.Description;
            }

            if (ApplyCategories)
            {
                Data.AudioBookInfo.Categories = SelectedSuggestion.Categories;
                UpdateMissingCategories();
            }

            if (ApplyRating)
            {
                Data.Rating = SelectedSuggestion.PercentageRating;
            }
        }

        private void ChapterOrderChanged(DraggableDroppedEventArgs<Chapter> obj)
        {
            var newIndex = obj.IndexInZone;
            var chapter = obj.Item;
            Data.AudioBookInfo.Chapters.Remove(chapter);
            Data.AudioBookInfo.Chapters.Insert(newIndex, chapter);
        }

        private void SortByName()
        {
            Data.AudioBookInfo.Chapters = Data.AudioBookInfo.Chapters.OrderBy(t => t.Name).ToList();
        }

        private void AddCategory(Category? obj)
        {
            if (obj != null && !Data.AudioBookInfo.Categories.Contains(obj))
            {
                Data.AudioBookInfo.Categories.Add(obj);
            }

            UpdateMissingCategories();
        }

        private void RemoveCategory(Category? obj)
        {
            if (obj != null && Data.AudioBookInfo.Categories.Contains(obj))
            {
                Data.AudioBookInfo.Categories.Remove(obj);
            }

            UpdateMissingCategories();
        }

        private async Task<Dictionary<string, string>> CreatePresignedUrl(ICollection<string> files)
        {
            var result = await Messenger.RequestAsync<AudioBookUploadStartRequest, IReadOnlyCollection<PreuploadFileInformation>>(
                new AudioBookUploadStartRequest(Id, files),
                PageCancellationTokenSource.Token);
            return result.ToDictionary(t => t.FileName, t => t.UploadUrl);
        }
    }
}
