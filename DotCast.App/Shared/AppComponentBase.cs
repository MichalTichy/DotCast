using Microsoft.AspNetCore.Components;

namespace DotCast.App.Shared
{
    public class AppComponentBase : ComponentBase, IAsyncDisposable
    {
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
