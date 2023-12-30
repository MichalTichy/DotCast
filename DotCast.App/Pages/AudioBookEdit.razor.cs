using Blazorise;
using DotCast.App.Shared;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Wolverine;

namespace DotCast.App.Pages
{
    [Authorize(Roles = "Admin")]
    public partial class AudioBookEdit : AppComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public required IMessageBus MessageBus { get; set; }

        [Parameter]
        public required string Id { get; set; }

        public AudioBook Data { get; set; } = new() { Id = "TMP", Name = "LOADING", AuthorName = "", Chapters = new List<Chapter>(0) };

        public IReadOnlyCollection<FoundBookInfo> Suggestions { get; set; } = new List<FoundBookInfo>(0).AsReadOnly();
        public ICollection<Category> MissingCategories = new List<Category>();
        public Category? SelectedCategory { get; set; }

        private Modal suggestionsModalRef = null!;


        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            UpdateMissingCategories();
            await base.OnInitializedAsync();
        }

        private async Task LoadData()
        {
            var request = new AudioBookDetailRequest(Id);
            var response = await MessageBus.InvokeAsync<AudioBook>(request);

            Data = response ?? throw new ArgumentException("Requested AudioBook not found");
        }

        public void UpdateMissingCategories()
        {
            var allCategories = Category.GetAll().ToList();
            MissingCategories = allCategories.Except(Data.Categories).ToList();
        }

        public async Task Prefill(FoundBookInfo suggestion)
        {
            Data.Name = suggestion.Title;
            Data.AuthorName = suggestion.Author;
            Data.SeriesName = suggestion.SeriesName;
            Data.OrderInSeries = suggestion.OrderInSeries;
            Data.Description = suggestion.Description;
            Data.Rating = suggestion.PercentageRating;
            Data.Categories = suggestion.Categories;

            await suggestionsModalRef.Close(CloseReason.None);
        }

        public async Task SaveAndExit()
        {
            await Save();
            NavigationManager.NavigateTo("/");
        }

        public async Task Save()
        {
            var request = new AudioBookEdited(Data);
            await MessageBus.InvokeAsync(request);
        }


        private async Task LoadSuggestions(string name)
        {
            var request = new AudiobookInfoSuggestionsRequest(name);
            var response = await MessageBus.InvokeAsync<IReadOnlyCollection<FoundBookInfo>>(request);
            Suggestions = response.ToList();
        }

        private void ChapterOrderChanged(DraggableDroppedEventArgs<Chapter> obj)
        {
            var newIndex = obj.IndexInZone;
            var chapter = obj.Item;
            Data.Chapters.Remove(chapter);
            Data.Chapters.Insert(newIndex, chapter);
        }

        private void SortByName()
        {
            Data.Chapters = Data.Chapters.OrderBy(t => t.Name).ToList();
        }

        private async Task ShowSuggestions()
        {
            await LoadSuggestions(Data.Name);
            await suggestionsModalRef.Show();
        }

        private void AddCategory(Category? obj)
        {
            if (obj != null && !Data.Categories.Contains(obj))
            {
                Data.Categories.Add(obj);
            }

            UpdateMissingCategories();
        }

        private void RemoveCategory(Category? obj)
        {
            if (obj != null && Data.Categories.Contains(obj))
            {
                Data.Categories.Remove(obj);
            }

            UpdateMissingCategories();
        }
    }
}
