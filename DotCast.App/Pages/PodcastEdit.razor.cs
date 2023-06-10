﻿using Blazorise;
using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.PodcastProvider.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace DotCast.App.Pages
{
    [Authorize(Roles = "Admin")]
    public partial class PodcastEdit : ComponentBase
    {
        [Inject]
        public IPodcastInfoProvider PodcastInfoProvider { get; set; } = null!;

        [Inject]
        public IPodcastFeedProvider PodcastFeedProvider { get; set; } = null!;

        [Inject]
        public IPodcastUploader PodcastUploader { get; set; } = null!;

        [Inject]
        public IPodcastDownloader PodcastDownloader { get; set; } = null!;
        [Inject]
        public IBookInfoProvider BookInfoProvider { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Parameter]
        public string Id { get; set; }

        public PodcastInfo Data { get; set; } = null!;

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
            Data = await PodcastInfoProvider.Get(Id) ?? throw new ArgumentException("Requested podcast not found");
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

        public async Task Save(PodcastInfo podcastInfo)
        {
            await PodcastInfoProvider.UpdatePodcastInfo(podcastInfo);
        }

        private PodcastInfo BuildUpdatedData()
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
                await using var stream = PodcastUploader.GetPodcastFileWriteStream(Id, file.Name, file.Type, out var _);
                await file.WriteToStreamAsync(stream);
            }

            await ResetImageUrl();
        }

        private async Task ResetImageUrl()
        {
            var feedCover = await PodcastFeedProvider.GetFeedCover(Id);
            var updatedData = Data with {ImageUrl = feedCover};
            await Save(updatedData);
            Data = updatedData;
            Image = Data.ImageUrl;
        }
    }
}