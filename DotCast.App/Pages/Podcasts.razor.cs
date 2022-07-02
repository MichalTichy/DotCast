using DotCast.PodcastProvider.Base;
using Microsoft.AspNetCore.Components;

namespace DotCast.App.Pages
{
    public partial class Podcasts
    {
        [Inject]
        public IPodcastInfoProvider PodcastInfoProvider { get; set; } = null!;

        [Inject]
        public IPodcastDownloader PodcastDownloader { get; set; } = null!;

        public IEnumerable<PodcastInfo> Data { get; set; } = Array.Empty<PodcastInfo>();

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        public void Download(string podcastName)
        {
            var url = PodcastDownloader.GetZipDownloadUrl(podcastName);
            NavigationManager.NavigateTo(url);
        }

        protected override async Task OnInitializedAsync()
        {
            Data = PodcastInfoProvider.GetPodcasts();
            await base.OnInitializedAsync();
        }
    }
}
