
using DotCast.App.Shared;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Wolverine;

namespace DotCast.App.Pages
{
    [Authorize]
    public partial class AudioBooks : AppComponentBase
    {
        [Inject]
        public required IJSRuntime Js { get; set; }

        [Inject]
        public required IMessageBus MessageBus { get; set; }

        public IReadOnlyList<AudioBook> Data { get; set; } = new List<AudioBook>().AsReadOnly();

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        public required AudioBooksStatistics AudioBooksStatistics { get; set; }

        public async Task Download(AudioBook info)
        {
            if (!string.IsNullOrWhiteSpace(info.ArchiveUrl))
            {
                await Js.InvokeVoidAsync("open", info.ArchiveUrl, "_blank");
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadStatistics();
            await LoadData();
            await base.OnInitializedAsync();
        }

        private async Task LoadStatistics()
        {
            var request = new AudioBooksStatisticsRequest();
            AudioBooksStatistics = await MessageBus.InvokeAsync<AudioBooksStatistics>(request);
        }

        private async Task LoadData(string? filter = null)
        {
            var request = new AudioBooksRetrievalRequest(filter);
            var result = await MessageBus.InvokeAsync<IReadOnlyList<AudioBook>>(request);
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
