using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotCast.App.Shared;

public partial class UxDialog : ComponentBase, IAsyncDisposable
{
    private readonly string dialogId = $"dialog-{Guid.NewGuid():N}";
    private DotNetObjectReference<UxDialog>? reference;
    [Inject] public required IJSRuntime Js { get; set; }
    [Parameter] public string Class { get; set; } = "confirmation-dialog";
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback Dismiss { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        reference = DotNetObjectReference.Create(this);
        await Js.InvokeVoidAsync("DotCastUi.openDialog", dialogId, reference);
    }

    [JSInvokable] public Task CloseDialog() => Dismiss.InvokeAsync();

    public async ValueTask DisposeAsync()
    {
        try { await Js.InvokeVoidAsync("DotCastUi.closeDialog", dialogId); }
        catch (JSDisconnectedException) { }
        reference?.Dispose();
    }
}
