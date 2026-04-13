using DotCast.App.Shared;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace DotCast.App.Pages
{
    [Authorize]
    public partial class NewAudioBook : AppPage
    {
        private const int TypingDelay = 450;
        private Timer? typingTimer;

        [Inject]
        public required IMessagePublisher Messenger { get; set; }

        public string AudioBookName { get; set; } = string.Empty;
        public string? AudioBookId { get; set; }
        public string? StatusMessage { get; set; }
        public bool ReadyForUpload => !string.IsNullOrWhiteSpace(AudioBookId);

        public override async ValueTask DisposeAsync()
        {
            typingTimer?.Dispose();
            await base.DisposeAsync();
        }

        private void BookNameTextChanged(string text)
        {
            AudioBookName = text;
            StatusMessage = null;
            typingTimer?.Dispose();
            typingTimer = new Timer(_ => _ = InvokeAsync(InitNewBookId), null, TypingDelay, Timeout.Infinite);
        }

        private async Task InitNewBookId()
        {
            if (string.IsNullOrWhiteSpace(AudioBookName))
            {
                AudioBookId = null;
                await SaveStateHasChangedAsync();
                return;
            }

            try
            {
                AudioBookId = await Messenger.RequestAsync<NewAudioBookIdRequest, string>(
                    new NewAudioBookIdRequest(AudioBookName),
                    PageCancellationTokenSource.Token);
                StatusMessage = null;
            }
            catch (Exception exception)
            {
                AudioBookId = null;
                StatusMessage = exception.Message;
            }

            await SaveStateHasChangedAsync();
        }

        private async Task<Dictionary<string, string>> GetPresignedUrls(ICollection<string> files)
        {
            if (string.IsNullOrWhiteSpace(AudioBookId))
            {
                throw new InvalidOperationException("Enter an audiobook name before uploading files.");
            }

            var result = await Messenger.RequestAsync<AudioBookUploadStartRequest, IReadOnlyCollection<PreuploadFileInformation>>(
                new AudioBookUploadStartRequest(AudioBookId, files),
                PageCancellationTokenSource.Token);
            StatusMessage = "Upload started.";
            return result.ToDictionary(t => t.FileName, t => t.UploadUrl);
        }
    }
}
