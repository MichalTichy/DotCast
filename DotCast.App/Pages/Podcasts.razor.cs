using DotCast.PodcastProvider.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotCast.App.Pages
{
    [Authorize]
    public partial class Podcasts
    {
        [Inject]
        public IJSRuntime Js { get; set; } = null!;

        [Inject]
        public IPodcastInfoProvider PodcastInfoProvider { get; set; } = null!;

        [Inject]
        public IPodcastDownloader PodcastDownloader { get; set; } = null!;

        public List<PodcastInfo> Data { get; set; } = new();

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        public PodcastsStatistics PodcastsStatistics { get; set; } = null!;

        public async Task Download(PodcastInfo info)
        {
            var url = await PodcastDownloader.GetZipDownloadUrl(info.Id);
            await Js.InvokeAsync<object>("open", url, "_blank");
        }

        protected override async Task OnInitializedAsync()
        {
            _ = Task.Run(async () =>
            {
                PodcastsStatistics = await PodcastInfoProvider.GetStatistics();
                await LoadData();
            });
            await base.OnInitializedAsync();
        }

        private async Task LoadData(string? filter = null)
        {
            await foreach (var podcastInfo in PodcastInfoProvider.GetPodcasts(filter))
            {
                Data.Add(podcastInfo);
                await InvokeAsync(StateHasChanged);
            }
        }

        private Task SearchTextChanged(string text)
        {
            Data.Clear();
            StateHasChanged();
            _ = Task.Run(async () => await LoadData(text));
            return Task.CompletedTask;
        }
    }
}