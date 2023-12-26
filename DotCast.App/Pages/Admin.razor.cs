using Blazorise;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Wolverine;

namespace DotCast.App.Pages
{
    [Authorize(Roles = "Admin")]
    public partial class Admin
    {
        public bool ReadyForUpload => !string.IsNullOrWhiteSpace(newAudioBookId);
        private Modal uploadModalRef = null!;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; } = null!;

        public string AudioBookName { get; set; } = null!;

        [Inject]
        public required IMessageBus MessageBus { get; set; }

        private async Task RestoreAudioBooksFromStorage()
        {
            var restoreFromFileSystemRequest = new RestoreFromFileSystemRequest();
            await MessageBus.SendAsync(restoreFromFileSystemRequest);
        }

        private Timer? typingTimer;
        private string? newAudioBookId;
        private const int TypingDelay = 1000; // Delay in millisecond

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
                newAudioBookId = await MessageBus.InvokeAsync<string>(request);
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception)
            {
                newAudioBookId = null;
            }
        }

        private async Task<Dictionary<string, string>> GetPresignedUrls(ICollection<string> arg)
        {
            var audioBookId = newAudioBookId ?? throw new InvalidOperationException();
            var request = new AudioBookUploadStartRequest(audioBookId, arg);
            var result = await MessageBus.InvokeAsync<IReadOnlyCollection<PreuploadFileInformation>>(request);

            return result.ToDictionary(t => t.FileName, t => t.UploadUrl);
        }
    }
}
