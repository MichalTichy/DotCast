using System.Text.Json;
using System.Text.RegularExpressions;
using DotCast.App.Shared;
using DotCast.App.Services;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace DotCast.App.Pages;

[Authorize]
public partial class AudioBookEdit : AppPage
{
    [Inject] public required IMessagePublisher Messenger { get; set; }
    [Inject] public required IJSRuntime Js { get; set; }
    [Parameter] public string Id { get; set; } = "";
    [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }
    [SupplyParameterFromQuery(Name = "title")] public string? InitialTitle { get; set; }
    private AudioBook? Data { get; set; }
    private string ActiveTab { get; set; } = "metadata";
    private string? Message { get; set; }
    private string? LoadError { get; set; }
    private string? loadedId;
    private string baseline = "";
    private bool IsSaving;
    private bool IsDeleting;
    private bool allowNavigation;
    private string? PendingLocation;
    private List<Chapter>? PreviousChapterOrder;
    private DateTime uploadStartedAt;
    private bool IsDirty => Data != null && baseline != Snapshot();
    private IEnumerable<Category> MissingCategories => Category.GetAll().Where(category => !Data!.AudioBookInfo.Categories.Contains(category));
    private string LibraryUrl => ReturnUrl is { } url && url.StartsWith("/") && !url.StartsWith("//") && NavigationManager.ToAbsoluteUri(url).AbsolutePath == "/" ? url : "/";
    private string Snapshot() => Data is null ? "" : JsonSerializer.Serialize(new
    {
        Data.AudioBookInfo.Name, Data.AudioBookInfo.AuthorName, Data.AudioBookInfo.SeriesName,
        Data.AudioBookInfo.OrderInSeries, Data.AudioBookInfo.Description, Data.Rating,
        Categories = Data.AudioBookInfo.Categories.Select(category => category.Name).Order(),
        Chapters = Data.AudioBookInfo.Chapters.Select(chapter => chapter.FileId)
    });
    protected override async Task OnParametersSetAsync()
    {
        if (loadedId == Id) return;
        loadedId = Id; Data = null; LoadError = null;
        try
        {
            Data = await Messenger.RequestAsync<AudioBookDetailRequest, AudioBook?>(new(Id), PageCancellationTokenSource.Token);
            if (Data is null) LoadError = Ux.Text("BookUnavailable");
            baseline = Snapshot();
            if (Data is not null && !string.IsNullOrWhiteSpace(InitialTitle))
            {
                Data.AudioBookInfo.Name = InitialTitle.Trim();
                Message = Ux.Text("UploadReview");
            }
        }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested) { LoadError = Ux.Text("LoadFailed"); }
    }
    private async Task<bool> SaveAsync()
    {
        if (Data is null || IsSaving) return false;
        if (string.IsNullOrWhiteSpace(Data.AudioBookInfo.Name)) { Message = Ux.Text("NameRequired"); return false; }
        if (Data.Rating < 0 || Data.Rating > 100 || Data.AudioBookInfo.OrderInSeries < 0) { Message = Ux.Text("InvalidNumbers"); return false; }
        IsSaving = true; Message = null;
        var savingSnapshot = Snapshot();
        try
        {
            await Messenger.ExecuteAsync(new SaveAudioBookRequest(Data), PageCancellationTokenSource.Token);
            baseline = savingSnapshot; Message = Ux.Text("Saved"); return true;
        }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested) { Message = Ux.Text("SaveFailed"); return false; }
        finally { IsSaving = false; }
    }
    private async Task Save() => await SaveAsync();
    private async Task SaveAndExit() { if (await SaveAsync()) Leave(LibraryUrl); }
    private void Leave(string location) { allowNavigation = true; PendingLocation = null; NavigationManager.NavigateTo(location); }
    private void BeforeNavigation(LocationChangingContext context)
    {
        if (allowNavigation || !IsDirty) return;
        context.PreventNavigation(); PendingLocation = context.TargetLocation;
    }
    private void Stay() => PendingLocation = null;
    private void DiscardAndLeave() { if (PendingLocation is { } target) Leave(target); }
    private async Task SaveAndLeave() { if (PendingLocation is { } target && await SaveAsync()) Leave(target); }
    private async Task DeleteAndExit()
    {
        if (Data is null || IsDeleting) return;
        if (!await Js.InvokeAsync<bool>("confirm", Ux.Format("ConfirmDelete", Data.AudioBookInfo.Name))) return;
        IsDeleting = true;
        try { await Messenger.ExecuteAsync(new AudioBookDeleteRequest(Id), PageCancellationTokenSource.Token); Leave(LibraryUrl); }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested) { Message = Ux.Text("DeleteFailed"); }
        finally { IsDeleting = false; }
    }
    private void SortByName()
    {
        PreviousChapterOrder = Data!.AudioBookInfo.Chapters.ToList();
        Data.AudioBookInfo.Chapters = Data.AudioBookInfo.Chapters.OrderBy(chapter => Regex.Replace(chapter.Name, @"\d+", match => match.Value.PadLeft(20, '0')), StringComparer.InvariantCultureIgnoreCase).ToList();
        Message = Ux.Text("ChapterOrderDraft");
    }
    private void UndoOrder() { if (PreviousChapterOrder != null) Data!.AudioBookInfo.Chapters = PreviousChapterOrder; PreviousChapterOrder = null; }
    private void MoveChapter(Chapter chapter, int offset)
    {
        var chapters = Data!.AudioBookInfo.Chapters;
        var index = chapters.IndexOf(chapter); var target = index + offset;
        if (target < 0 || target >= chapters.Count) return;
        PreviousChapterOrder = chapters.ToList(); chapters.RemoveAt(index); chapters.Insert(target, chapter);
    }
    private void AddCategory(ChangeEventArgs e)
    {
        var category = MissingCategories.FirstOrDefault(category => category.Name == e.Value?.ToString());
        if (category != null) Data!.AudioBookInfo.Categories.Add(category);
    }
    private void RemoveCategory(Category category) => Data!.AudioBookInfo.Categories.Remove(category);
    private async Task<Dictionary<string, string>> CreatePresignedUrl(ICollection<string> files)
    {
        if (IsDirty) throw new InvalidOperationException(Ux.Text("SaveBeforeUpload"));
        if (!await Js.InvokeAsync<bool>("confirm", Ux.Text("ConfirmUpload"))) throw new InvalidOperationException(Ux.Text("UploadCancelled"));
        uploadStartedAt = DateTime.UtcNow;
        var result = await Messenger.RequestAsync<AudioBookUploadStartRequest, IReadOnlyCollection<PreuploadFileInformation>>(new(Id, files), PageCancellationTokenSource.Token);
        return result.ToDictionary(file => file.FileName, file => file.UploadUrl);
    }
    private void UploadCompleted()
    {
        Message = Ux.Text("UploadProcessing");
        _ = RefreshAfterUploadAsync(Id, uploadStartedAt);
    }
    private async Task RefreshAfterUploadAsync(string id, DateTime startedAt)
    {
        try
        {
            for (var attempt = 0; attempt < 120; attempt++)
            {
                await Task.Delay(1000, PageCancellationTokenSource.Token);
                var job = ProcessingMonitor.RecentJobs.FirstOrDefault(job => job.AudioBookId == id && job.TimestampUtc >= startedAt && job.Succeeded.HasValue);
                if (job is null) continue;
                if (job.Succeeded == false) { await InvokeAsync(() => { Message = Ux.Text("UploadProcessingFailed"); StateHasChanged(); }); return; }
                var updated = await Messenger.RequestAsync<AudioBookDetailRequest, AudioBook?>(new(id), PageCancellationTokenSource.Token);
                await InvokeAsync(() =>
                {
                    if (Data?.Id != id) return;
                    if (!IsDirty && updated != null) { Data = updated; baseline = Snapshot(); PreviousChapterOrder = null; Message = Ux.Text("UploadReady"); }
                    else Message = Ux.Text("UploadDraftRetained");
                    StateHasChanged();
                });
                return;
            }
            await InvokeAsync(() => { Message = Ux.Text("ProcessingTakingLonger"); StateHasChanged(); });
        }
        catch (OperationCanceledException) { }
        catch (Exception) when (!PageCancellationTokenSource.IsCancellationRequested)
        { await InvokeAsync(() => { Message = Ux.Text("ProcessingCheckFailed"); StateHasChanged(); }); }
    }
}
