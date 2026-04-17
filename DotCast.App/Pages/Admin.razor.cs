using Blazorise;
using DotCast.App.Services;
using DotCast.App.Shared;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Processing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace DotCast.App.Pages
{
    [Authorize(Roles = UserRoleManager.AdminRoleName)]
    public partial class Admin : AppPage
    {
        private const int TypingDelay = 1000; // Delay in millisecond
        private Modal uploadModalRef = null!;
        private Modal uploadMultipleModalRef = null!;

        private Timer? typingTimer;
        private string? newAudioBookId;
        public bool ReadyForUpload => !string.IsNullOrWhiteSpace(newAudioBookId);


        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; } = null!;

        public string AudioBookName { get; set; } = null!;

        [Inject]
        public required IMessagePublisher Messenger { get; set; }

        [Inject]
        public required ProcessingPipeline Test { get; set; }

        public bool IsProcessingRunning { get; set; }
        public int CountOfRunningProcessings { get; set; }
        public bool IsApplyingSuggestions { get; set; }
        public string? SuggestionUpdateMessage { get; set; }
        public IReadOnlyList<ActivePlaybackInfo> ActivePlaybacks { get; set; } = [];
        public bool IsLoadingActivePlaybacks { get; set; }
        public string ActiveTab { get; set; } = "maintenance";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadActivePlaybacks();
        }


        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                _ = Task.Run(async () =>
                {
                    var cancellationToken = PageCancellationTokenSource.Token;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await UpdateProcessingInfo();
                        await Task.Delay(250, cancellationToken);
                    }
                });
            }

            base.OnAfterRender(firstRender);
        }

        private async Task UpdateProcessingInfo()
        {
            IsProcessingRunning = ProcessingMonitor.IsProcessingRunning;
            CountOfRunningProcessings = ProcessingMonitor.CountOfRunningProcessings;
            await SaveStateHasChangedAsync();
        }

        private async Task RestoreAudioBooksFromStorage()
        {
            if (IsProcessingRunning)
            {
                return;
            }

            var restoreFromFileSystemRequest = new RestoreFromFileSystemRequest();
            await Messenger.PublishAsync(restoreFromFileSystemRequest);
        }

        private async Task ReprocessAudioBooksFromStorage(bool unzipFirst)
        {
            if (IsProcessingRunning)
            {
                return;
            }

            var restoreFromFileSystemRequest = new ReprocessAllAudioBooksRequest(unzipFirst);
            await Messenger.PublishAsync(restoreFromFileSystemRequest);
        }

        private async Task ApplyStrongSuggestions()
        {
            if (IsProcessingRunning || IsApplyingSuggestions)
            {
                return;
            }

            IsApplyingSuggestions = true;
            SuggestionUpdateMessage = "Looking up strong metadata matches...";
            await SaveStateHasChangedAsync();

            try
            {
                var result = await Messenger.RequestAsync<ApplyAudiobookSuggestionsToAllRequest, ApplyAudiobookSuggestionsToAllResult>(
                    new ApplyAudiobookSuggestionsToAllRequest(),
                    PageCancellationTokenSource.Token);

                SuggestionUpdateMessage =
                    $"Checked {result.Total} audiobook(s). Strong matches: {result.StrongMatches}. Updated: {result.Updated}. No suggestions: {result.NoSuggestions}. Weak matches skipped: {result.WeakMatches}.";
            }
            catch (Exception exception) when (!PageCancellationTokenSource.IsCancellationRequested)
            {
                SuggestionUpdateMessage = $"Suggestion update failed: {exception.Message}";
            }
            finally
            {
                IsApplyingSuggestions = false;
                await SaveStateHasChangedAsync();
            }
        }

        private async Task ShowTab(string tab)
        {
            ActiveTab = tab;
            if (tab == "playbacks")
            {
                await LoadActivePlaybacks();
            }
        }

        private async Task LoadActivePlaybacks()
        {
            IsLoadingActivePlaybacks = true;
            await SaveStateHasChangedAsync();

            try
            {
                ActivePlaybacks = await Messenger.RequestAsync<ActivePlaybacksRequest, IReadOnlyList<ActivePlaybackInfo>>(
                    new ActivePlaybacksRequest(),
                    PageCancellationTokenSource.Token);
            }
            finally
            {
                IsLoadingActivePlaybacks = false;
                await SaveStateHasChangedAsync();
            }
        }

        private static string PlaybackStatusClass(PlaybackStatus status)
        {
            return status switch
            {
                PlaybackStatus.InfoRetrieved => "is-info",
                PlaybackStatus.InProgress => "is-progress",
                PlaybackStatus.CloseToFinished => "is-close",
                PlaybackStatus.Finished => "is-finished",
                _ => string.Empty
            };
        }

        private void BookNameTextChanged(string text)
        {
            typingTimer?.Dispose();
            typingTimer = new Timer(state => _ = InitNewBookId(text), null, TypingDelay, Timeout.Infinite);
        }

        private async Task InitNewBookId(string text)
        {
            try
            {
                AudioBookName = text;
                var request = new NewAudioBookIdRequest(text);
                newAudioBookId = await Messenger.RequestAsync<NewAudioBookIdRequest, string>(request, PageCancellationTokenSource.Token);
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception)
            {
                newAudioBookId = null;
            }
        }

        private async Task<Dictionary<string, string>> GetPresignedUrls(string audioBookId, ICollection<string> files)
        {
            var request = new AudioBookUploadStartRequest(audioBookId, files);
            var result = await Messenger.RequestAsync<AudioBookUploadStartRequest, IReadOnlyCollection<PreuploadFileInformation>>(request, PageCancellationTokenSource.Token);

            return result.ToDictionary(t => t.FileName, t => t.UploadUrl);
        }

        private async Task<Dictionary<string, string>> GetPresignedUrls(ICollection<string> files)
        {
            var urls = new Dictionary<string, string>();
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var idRequest = new NewAudioBookIdRequest(name);
                var audioBookId = await Messenger.RequestAsync<NewAudioBookIdRequest, string>(idRequest);

                var request = new AudioBookUploadStartRequest(audioBookId, new[] { file });
                var result = await Messenger.RequestAsync<AudioBookUploadStartRequest, IReadOnlyCollection<PreuploadFileInformation>>(request, PageCancellationTokenSource.Token);

                var fileResult = result.Single();
                urls.Add(fileResult.FileName, fileResult.UploadUrl);
            }

            return urls;
        }
    }
}
