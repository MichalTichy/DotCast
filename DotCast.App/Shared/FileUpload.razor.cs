using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotCast.App.Shared;

public partial class FileUpload : ComponentBase, IAsyncDisposable
{
    private readonly string componentId = $"upload_{Guid.NewGuid():N}";
    private string containerId => $"{componentId}_container";
    private string filePickerId => $"{componentId}_picker";
    private string progressContainerId => $"{componentId}_progress";
    private string helpId => $"{componentId}_help";
    private DotNetObjectReference<FileUpload>? reference;
    [Inject] public required IJSRuntime Js { get; set; }
    [Parameter, EditorRequired] public Func<ICollection<string>, Task<Dictionary<string, string>>> PresignedUrlFactory { get; set; } = null!;
    [Parameter] public string Label { get; set; } = Ux.Text("UploadFiles");
    [Parameter] public bool Multiple { get; set; } = true;
    [Parameter] public EventCallback Completed { get; set; }
    [Parameter] public EventCallback Started { get; set; }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        reference = DotNetObjectReference.Create(this);
        await Js.InvokeVoidAsync("UploadHelpers.register", componentId, reference, filePickerId, progressContainerId,
            new { preparing = Ux.Text("UploadPreparing"), uploaded = Ux.Text("Uploaded"), failed = Ux.Text("UploadFailed"), retry = Ux.Text("Retry"), processing = Ux.Text("UploadProcessing"), empty = Ux.Text("EmptyFiles"), duplicate = Ux.Text("DuplicateFiles") });
    }
    [JSInvokable] public async Task<Dictionary<string, string>> GeneratePresignedUrls(List<string> names)
    {
        var urls = await PresignedUrlFactory(names);
        await Started.InvokeAsync();
        return urls;
    }
    [JSInvokable] public Task UploadCompleted() => Completed.InvokeAsync();
    public async ValueTask DisposeAsync()
    {
        try { await Js.InvokeVoidAsync("UploadHelpers.unregister", componentId); } catch (JSDisconnectedException) { }
        reference?.Dispose();
    }
}
