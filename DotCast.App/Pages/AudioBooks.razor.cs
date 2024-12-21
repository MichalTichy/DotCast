
using DotCast.App.Shared;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Library;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotCast.App.Pages
{
    [Authorize]
    public partial class AudioBooks : AppPage
    {
        [Inject]
        public required IJSRuntime Js { get; set; }

        [Inject]
        public required ILibraryApiInformationProvider LibraryApiInfoProvider { get; set; }

        [Inject]
        public required IMessagePublisher Messenger { get; set; }


        public IReadOnlyList<AudioBook> Data { get; set; } = new List<AudioBook>().AsReadOnly();

        public required AudioBooksStatistics AudioBooksStatistics { get; set; }

        public async Task CopyRssAsync(AudioBook info)
        {
            var url = await LibraryApiInfoProvider.GetFeedUrlAsync(info.Id);
            await Js.InvokeVoidAsync("copyToClipboard", url);
        }
        public async Task Download(AudioBook info)
        {
            if (!string.IsNullOrWhiteSpace(info.AudioBookInfo.ArchiveUrl))
            {
                await Js.InvokeVoidAsync("open", info.AudioBookInfo.ArchiveUrl, "_blank");
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var tasks = new[]
            {
                LoadStatistics(),
                LoadData()
            };
            await Task.WhenAll(tasks);
        }

        private async Task LoadStatistics()
        {
            var request = new AudioBooksStatisticsRequest();
            AudioBooksStatistics = await Messenger.RequestAsync<AudioBooksStatisticsRequest, AudioBooksStatistics>(request, PageCancellationTokenSource.Token);
        }

        private async Task LoadData(string? filter = null)
        {
            var request = new AudioBooksRetrievalRequest(filter);
            var result = await Messenger.RequestAsync<AudioBooksRetrievalRequest, IReadOnlyList<AudioBook>>(request, PageCancellationTokenSource.Token);
            Data = result;
            await SaveStateHasChangedAsync();
        }

        private Timer? typingTimer;
        private const int TypingDelay = 1000; // Delay in millisecond

        private void SearchTextChanged(string text)
        {
            typingTimer?.Dispose();
            typingTimer = new Timer(state => _ = LoadData(text), null, TypingDelay, Timeout.Infinite);
        }
    }
}