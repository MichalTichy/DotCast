using Blazorise;
using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.AudioBookProvider.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace DotCast.App.Pages
{
    [Authorize(Roles = "Admin")]
    public partial class AudioBookEdit : ComponentBase
    {
        [Inject]
        public IAudioBookInfoProvider AudioBookInfoProvider { get; set; } = null!;

        [Inject]
        public IAudioBookFeedProvider AudioBookFeedProvider { get; set; } = null!;

        [Inject]
        public IAudioBookUploader AudioBookUploader { get; set; } = null!;

        [Inject]
        public IAudioBookDownloader AudioBookDownloader { get; set; } = null!;
        [Inject]
        public IBookInfoProvider BookInfoProvider { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Parameter]
        public string Id { get; set; }

        public AudioBookInfo Data { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? SeriesName { get; set; }
        public int OrderInSeries { get; set; }
        public int Rating { get; set; }
        public string AuthorName { get; set; } = null!;
        public string? Image { get; set; }
        public string? Description { get; set; }
        public List<BookInfo> Suggestions { get; set; } = new List<BookInfo>();

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await base.OnInitializedAsync();
        }

        private async Task LoadData()
        {
            Data = await AudioBookInfoProvider.Get(Id) ?? throw new ArgumentException("Requested AudioBook not found");
            InitDataProperties();
            _ = Task.Run(async () =>
            {
                await LoadSuggestions();

            });
        }

        private async Task LoadSuggestions()
        {
            await foreach (var bookInfo in BookInfoProvider.GetBookInfoAsync(Data.Name))
            {
                Suggestions.Add(bookInfo);
                await InvokeAsync(StateHasChanged);
            }
        }

        public void Prefill(BookInfo suggestion)
        {
            Name = suggestion.Title;
            AuthorName = suggestion.Author;
            SeriesName = suggestion.SeriesName;
            OrderInSeries= suggestion.OrderInSeries;
            Description = suggestion.Description;
            Rating = suggestion.PercentageRating;
        }

        private void InitDataProperties()
        {
            Name = Data.Name;
            AuthorName = Data.AuthorName;
            Image = Data.ImageUrl;
            SeriesName = Data.SeriesName;
            OrderInSeries = Data.OrderInSeries;
            Description = Data.Description;
            Rating = Data.Rating;
        }

        public async Task SaveAndExit()
        {
            var updatedData = BuildUpdatedData();
            await Save(updatedData);
            NavigationManager.NavigateTo("/");
        }

        public async Task Save(AudioBookInfo AudioBookInfo)
        {
            await AudioBookInfoProvider.UpdateAudioBookInfo(AudioBookInfo);
        }

        private AudioBookInfo BuildUpdatedData()
        {
            return Data with
            {
                Name = Name,
                AuthorName = AuthorName,
                SeriesName = !string.IsNullOrWhiteSpace(SeriesName) ? SeriesName : null,
                OrderInSeries = OrderInSeries,
                Description = !string.IsNullOrWhiteSpace(Description) ? Description : null,
                Rating = Rating
            };
        }

        private async Task FilesSelected(FileChangedEventArgs e)
        {
            foreach (var file in e.Files)
            {
                await using var stream = AudioBookUploader.GetAudioBookFileWriteStream(Id, file.Name, file.Type, out var _);
                await file.WriteToStreamAsync(stream);
            }

            await ResetImageUrl();
        }

        private async Task ResetImageUrl()
        {
            var feedCover = await AudioBookFeedProvider.GetFeedCover(Id);
            var updatedData = Data with {ImageUrl = feedCover};
            await Save(updatedData);
            Data = updatedData;
            Image = Data.ImageUrl;
        }
    }
}
