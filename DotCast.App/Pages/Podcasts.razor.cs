using DotCast.PodcastProvider.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotCast.App.Pages
{
    public partial class Podcasts
    {
        [Inject]
        public IJSRuntime Js { get; set; }

        [Inject]
        public IPodcastInfoProvider PodcastInfoProvider { get; set; } = null!;

        [Inject]
        public IPodcastDownloader PodcastDownloader { get; set; } = null!;

        public IEnumerable<PodcastInfo> Data { get; set; } = Array.Empty<PodcastInfo>();

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        public async Task Download(PodcastInfo info)
        {
            var url = await PodcastDownloader.GetZipDownloadUrl(info.Id);
            await Js.InvokeAsync<object>("open", url, "_blank");
        }

        protected override async Task OnInitializedAsync()
        {
            Data = PodcastInfoProvider.GetPodcasts();
            await base.OnInitializedAsync();
        }
    }
}