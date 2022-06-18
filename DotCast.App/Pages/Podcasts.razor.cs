using DotCast.PodcastProvider.Base;
using Microsoft.AspNetCore.Components;

namespace DotCast.App.Pages
{
    public partial class Podcasts
    {
        [Inject]
        public IPodcastInfoProvider PodcastInfoProvider { get; set; } = null!;

        public IEnumerable<PodcastInfo> Data { get; set; } = Array.Empty<PodcastInfo>();

        protected override async Task OnInitializedAsync()
        {
            Data = PodcastInfoProvider.GetPodcasts();
            await base.OnInitializedAsync();
        }
    }
}
