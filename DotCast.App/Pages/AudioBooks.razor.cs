using DotCast.AudioBookProvider.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotCast.App.Pages
{
    [Authorize]
    public partial class AudioBooks
    {
        [Inject]
        public IJSRuntime Js { get; set; } = null!;

        [Inject]
        public IAudioBookInfoProvider AudioBookInfoProvider { get; set; } = null!;

        [Inject]
        public IAudioBookDownloader AudioBookDownloader { get; set; } = null!;

        public List<AudioBookInfo> Data { get; set; } = new();

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        public AudioBooksStatistics AudioBooksStatistics { get; set; } = null!;

        public async Task Download(AudioBookInfo info)
        {
            var url = await AudioBookDownloader.GetZipDownloadUrl(info.Id);
            await Js.InvokeAsync<object>("open", url, "_blank");
        }

        protected override async Task OnInitializedAsync()
        {
            _ = Task.Run(async () =>
            {
                AudioBooksStatistics = await AudioBookInfoProvider.GetStatistics();
                await LoadData();
            });
            await base.OnInitializedAsync();
        }

        private async Task LoadData(string? filter = null)
        {
            await foreach (var AudioBookInfo in AudioBookInfoProvider.GetAudioBooks(filter))
            {
                Data.Add(AudioBookInfo);
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
