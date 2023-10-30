using Blazorise;
using DotCast.AudioBookInfo;
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
        public IBookInfoProvider BookInfoProvider { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Parameter]
        public string Id { get; set; }

        public AudioBook Data { get; set; } = new() {Id = "TMP", Name = "LOADING", AuthorName = "", Chapters = new List<Chapter>()};

        public List<BookInfo> Suggestions { get; set; } = new List<BookInfo>();

        private Modal suggestionsModalRef = null!;


        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await base.OnInitializedAsync();
        }

        private async Task LoadData()
        {
            Data = await AudioBookInfoProvider.Get(Id) ?? throw new ArgumentException("Requested AudioBook not found");
            _ = Task.Run(async () => await LoadSuggestions());
        }


        public async Task Prefill(BookInfo suggestion)
        {
            Data.Name = suggestion.Title;
            Data.AuthorName = suggestion.Author;
            Data.SeriesName = suggestion.SeriesName;
            Data.OrderInSeries = suggestion.OrderInSeries;
            Data.Description = suggestion.Description;
            Data.Rating = suggestion.PercentageRating;
            await suggestionsModalRef.Close(CloseReason.None);
        }


        public async Task SaveAndExit()
        {
            await Save();
            NavigationManager.NavigateTo("/");
        }

        public async Task Save()
        {
            await AudioBookInfoProvider.UpdateAudioBook(Data);
        }


        private async Task FilesSelected(FileChangedEventArgs e)
        {
            foreach (var file in e.Files)
            {
                await using var stream = AudioBookUploader.GetAudioBookFileWriteStream(Id, file.Name, file.Type, out var _);
                await file.WriteToStreamAsync(stream);
            }
        }

        private async Task LoadSuggestions()
        {
            await foreach (var bookInfo in BookInfoProvider.GetBookInfoAsync(Data.Name))
            {
                Suggestions.Add(bookInfo);
                await InvokeAsync(StateHasChanged);
            }
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
    }
}
