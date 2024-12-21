using Blazorise;
using DotCast.App.Services;
using DotCast.App.Shared;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Components;
using Shared.Infrastructure.CurrentUserProvider;

namespace DotCast.App.Pages
{
    public partial class UserProfile : AppPage
    {
        public string UserName { get; set; } = string.Empty;
        public string LibraryName { get; set; } = string.Empty;
        public ICollection<ShareInfo> SharedLibrariesWith { get; set; } = Array.Empty<ShareInfo>();
        public string NewShare { get; set; } = string.Empty;

        [Inject]
        public required ICurrentUserProvider<UserInfo> UserProvider { get; set; }

        [Inject]
        public required UserManager UserManager { get; set; }

        public bool IsProcessingRunning { get; set; }
        public string AudioBookName { get; set; } = null!;
        private const int TypingDelay = 1000; // Delay in millisecond

        private Modal uploadModalRef = null!;
        private Modal uploadMultipleModalRef = null!;

        private Timer? typingTimer;
        private string? newAudioBookId;
        public bool ReadyForUpload => !string.IsNullOrWhiteSpace(newAudioBookId);

        [Inject]
        public required IMessagePublisher Messenger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var user = await UserProvider.GetCurrentUserRequiredAsync();
            UserName = user.Name!;
            LibraryName = user.UsersLibraryName;

            _ = Task.Run(LoadSharingInfo);
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
            await SaveStateHasChangedAsync();
        }

        private async Task LoadSharingInfo()
        {
            var user = await UserProvider.GetCurrentUserRequiredAsync();

            var users = await UserManager.MathUserByLibraryCodeAsync(user.SharedLibraries);
            SharedLibrariesWith = users.Select(t => new ShareInfo(t.Id, t.UserName!, t.UsersLibraryName)).ToList();
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

        public async Task ShareAsync()
        {
            var currentUser = await CurrentUserProvider.GetCurrentUserRequiredAsync();
            try
            {
                await UserManager.ShareLibraryAsync(currentUser.Id, NewShare);
            }
            catch (ArgumentException e)
            {
                //Invalid code
            }
        }

        public async Task UnShareAsync(ShareInfo shareInfo)
        {
            var currentUser = await CurrentUserProvider.GetCurrentUserRequiredAsync();
            try
            {
                await UserManager.UnShareLibraryAsync(currentUser.Id, shareInfo.LibraryCode);
            }
            catch (ArgumentException e)
            {
                //Invalid code
            }
        }

        public record ShareInfo(string UserId, string UserName, string LibraryCode);
    }
}
