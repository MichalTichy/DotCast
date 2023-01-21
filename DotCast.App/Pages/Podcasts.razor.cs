using DotCast.PodcastProvider.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotCast.App.Pages
{
    public partial class Podcasts
    {
        [Inject]
        public IJSRuntime Js { get; set; } = null!;

        [Inject]
        public IPodcastInfoProvider PodcastInfoProvider { get; set; } = null!;

        [Inject]
        public IPodcastDownloader PodcastDownloader { get; set; } = null!;

        public IEnumerable<PodcastInfo> Data { get; set; } = Array.Empty<PodcastInfo>();

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
            await LoadData();
            await base.OnInitializedAsync();
        }

        private async Task LoadData(string? filter = null)
        {
            Data = await PodcastInfoProvider.GetPodcasts(filter).ToListAsync();
            PodcastsStatistics = await PodcastInfoProvider.GetStatistics();
        }

        private async Task SearchTextChanged(string text)
        {
            await LoadData(text);
        }
    }
}