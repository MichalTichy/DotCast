using DotCast.App.Shared;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace DotCast.App.Pages;

[Authorize]
public partial class NewAudioBook : AppPage
{
    [Inject] public required IMessagePublisher Messenger { get; set; }
    private string AudioBookName { get; set; } = "";
    private string? AudioBookId;
    private string? ExistingId;
    private string? StatusMessage;
    private bool IsChecking;
    private bool IsProcessingRunning;
    private bool IsUploading;
    private Timer? typingTimer;
    private int nameVersion;
    private bool ReadyForUpload => !IsChecking && ExistingId is null && !string.IsNullOrEmpty(AudioBookId);
    private void BookNameTextChanged(ChangeEventArgs e)
    {
        AudioBookName = e.Value?.ToString() ?? "";
        AudioBookId = ExistingId = StatusMessage = null;
        IsChecking = !string.IsNullOrWhiteSpace(AudioBookName);
        typingTimer?.Dispose();
        var version = ++nameVersion;
        if (IsChecking) typingTimer = new Timer(_ => _ = InvokeAsync(() => CheckNameAsync(version)), null, 450, Timeout.Infinite);
    }
    private async Task CheckNameAsync(int version)
    {
        try
        {
            var result = await Messenger.RequestAsync<AudioBookNameAvailabilityRequest, AudioBookNameAvailability>(new(AudioBookName), PageCancellationTokenSource.Token);
            if (version != nameVersion) return;
            AudioBookId = result.Id;
            ExistingId = result.Exists ? result.Id : null;
            StatusMessage = result.Exists ? Ux.Text("BookExists") : null;
        }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested)
        { if (version == nameVersion) StatusMessage = Ux.Text("NameCheckFailed"); }
        finally { if (version == nameVersion) { IsChecking = false; await SaveStateHasChangedAsync(); } }
    }
    private async Task<Dictionary<string, string>> GetPresignedUrls(ICollection<string> files)
    {
        if (!ReadyForUpload) throw new InvalidOperationException(Ux.Text("NameRequired"));
        IsUploading = true;
        await SaveStateHasChangedAsync();
        try
        {
            var result = await Messenger.RequestAsync<AudioBookUploadStartRequest, IReadOnlyCollection<PreuploadFileInformation>>(new(AudioBookId!, files), PageCancellationTokenSource.Token);
            return result.ToDictionary(file => file.FileName, file => file.UploadUrl);
        }
        catch { IsUploading = false; await SaveStateHasChangedAsync(); throw; }
    }
    private void UploadCompleted()
    {
        IsUploading = false; IsProcessingRunning = true; StatusMessage = Ux.Text("UploadProcessing");
        _ = WatchUntilReadyAsync(AudioBookId!);
    }
    private async Task WatchUntilReadyAsync(string id)
    {
        try
        {
            for (var attempt = 0; attempt < 120; attempt++)
            {
                await Task.Delay(1000, PageCancellationTokenSource.Token);
                var book = await Messenger.RequestAsync<AudioBookDetailRequest, AudioBook?>(new(id), PageCancellationTokenSource.Token);
                if (book is null) continue;
                await InvokeAsync(() => NavigationManager.NavigateTo($"/AudioBook/{Uri.EscapeDataString(id)}/edit?title={Uri.EscapeDataString(AudioBookName.Trim())}")); return;
            }
            await InvokeAsync(() => { IsProcessingRunning = false; StatusMessage = Ux.Text("ProcessingTakingLonger"); StateHasChanged(); });
        }
        catch (OperationCanceledException) { }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested)
        { await InvokeAsync(() => { IsProcessingRunning = false; StatusMessage = Ux.Text("ProcessingCheckFailed"); StateHasChanged(); }); }
    }
    public override async ValueTask DisposeAsync() { typingTimer?.Dispose(); await base.DisposeAsync(); }
}
