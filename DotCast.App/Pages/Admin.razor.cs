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