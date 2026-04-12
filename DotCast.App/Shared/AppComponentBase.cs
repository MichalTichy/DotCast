using DotCast.Infrastructure.AppUser;
using Microsoft.AspNetCore.Components;
using DotCast.Infrastructure.AppUser.Identity;
using DotCast.Infrastructure.CurrentUserProvider;

namespace DotCast.App.Shared
{
    public class AppComponentBase : ComponentBase, IAsyncDisposable
    {
        [Inject]
        public required ICurrentUserProvider<UserInfo> CurrentUserProvider { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        public CancellationTokenSource PageCancellationTokenSource { get; } = new();

        public async Task SaveStateHasChangedAsync()
        {
            await InvokeAsync(StateHasChanged);
        }

        public virtual async ValueTask DisposeAsync()
        {
            await PageCancellationTokenSource.CancelAsync();
        }
    }
}
