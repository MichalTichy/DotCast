using DotCast.App.Services;
using DotCast.App.Shared;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace DotCast.App.Pages;

[Authorize(Roles = UserRoleManager.AdminRoleName)]
public partial class Admin : AppPage
{
    [Inject] public required IMessagePublisher Messenger { get; set; }
    private string ActiveTab = "maintenance";
    private string? Message;
    private string? PendingOperation;
    private ActivePlaybackInfo? PendingPlayback;
    private bool IsBusy;
    private bool IsLoadingActivePlaybacks;
    private IReadOnlyList<ActivePlaybackInfo> ActivePlaybacks = [];
    private IReadOnlyList<MaintenanceBookPreview> MaintenanceBookPreviews = [];
    private string UserFilter = "";
    private string StatusFilter = "";
    private bool RecentFirst = true;
    private bool IsProcessingRunning => ProcessingMonitor.IsProcessingRunning || ProcessingMonitor.RecentJobs.Any(job => job.Succeeded is null);
    private IEnumerable<ActivePlaybackInfo> FilteredPlaybacks => RecentFirst ? MatchingPlaybacks.OrderByDescending(LastActivity) : MatchingPlaybacks.OrderBy(LastActivity);
    private IEnumerable<ActivePlaybackInfo> MatchingPlaybacks => ActivePlaybacks.Where(playback =>
        (UserFilter.Length == 0 || playback.UserId == UserFilter) && (StatusFilter.Length == 0 || playback.Status.ToString() == StatusFilter));
    private static DateTime LastActivity(ActivePlaybackInfo playback) => playback.LastFileDownloadedAt > playback.LastRssGeneratedAt ? playback.LastFileDownloadedAt.Value : playback.LastRssGeneratedAt;
    private static string Timestamp(DateTime? value) => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm 'UTC'") : Ux.Text("Never");
    private static string PlaybackLabel(PlaybackStatus status) => Ux.Text("Playback_" + status);
    private string BookTitle(string id) => MaintenanceBookPreviews.FirstOrDefault(book => book.Id == id)?.Title ?? Ux.Text("UnnamedStoredBook");
    protected override async Task OnInitializedAsync()
    {
        await LoadActivePlaybacks();
        try { MaintenanceBookPreviews = await Messenger.RequestAsync<MaintenancePreviewRequest, IReadOnlyList<MaintenanceBookPreview>>(new(), PageCancellationTokenSource.Token); }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested) { Message = Ux.Text("MaintenancePreviewFailed"); }
    }
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) _ = PollProcessingAsync();
        return Task.CompletedTask;
    }
    private async Task PollProcessingAsync()
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(PageCancellationTokenSource.Token)) await SaveStateHasChangedAsync();
        }
        catch (OperationCanceledException) { }
    }
    private async Task ShowTab(string tab) { ActiveTab = tab; Message = null; if (tab == "playbacks") await LoadActivePlaybacks(); }
    private async Task LoadActivePlaybacks()
    {
        if (IsLoadingActivePlaybacks) return;
        IsLoadingActivePlaybacks = true;
        try { ActivePlaybacks = await Messenger.RequestAsync<ActivePlaybacksRequest, IReadOnlyList<ActivePlaybackInfo>>(new(), PageCancellationTokenSource.Token); }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested) { Message = Ux.Text("PlaybackLoadFailed"); }
        finally { IsLoadingActivePlaybacks = false; }
    }
    private async Task MarkFinished()
    {
        if (PendingPlayback is not { } playback || IsBusy) return;
        IsBusy = true;
        try
        {
            await Messenger.ExecuteAsync(new AudioBookPlaybackMarkedFinished(playback.AudioBookId, playback.UserId, DateTime.UtcNow), PageCancellationTokenSource.Token);
            PendingPlayback = null; Message = Ux.Text("PlaybackFinished"); await LoadActivePlaybacks();
        }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested) { Message = Ux.Text("PlaybackFinishFailed"); }
        finally { IsBusy = false; }
    }
    private async Task ReviewOperation(string operation)
    {
        if (IsBusy || IsProcessingRunning) return;
        IsBusy = true; Message = null;
        try { MaintenanceBookPreviews = await Messenger.RequestAsync<MaintenancePreviewRequest, IReadOnlyList<MaintenanceBookPreview>>(new(), PageCancellationTokenSource.Token); PendingOperation = operation; }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested) { Message = Ux.Text("MaintenancePreviewFailed"); }
        finally { IsBusy = false; }
    }
    private async Task RunOperation()
    {
        if (PendingOperation is not { } operation || IsBusy || IsProcessingRunning) return;
        IsBusy = true;
        try
        {
            if (operation == "restore") await Messenger.PublishAsync(new RestoreFromFileSystemRequest());
            else await Messenger.PublishAsync(new ReprocessAllAudioBooksRequest(operation == "unzip"));
            PendingOperation = null; Message = Ux.Format(MaintenanceBookPreviews.Count == 1 ? "MaintenanceQueuedOne" : "MaintenanceQueued", MaintenanceBookPreviews.Count);
        }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested) { Message = Ux.Text("MaintenanceFailed"); }
        finally { IsBusy = false; }
    }
}
