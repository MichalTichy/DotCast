using DotCast.App.Shared;
using DotCast.Infrastructure.AppUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotCast.App.Pages;

[Authorize]
public partial class UserProfile : AppPage
{
    [Inject] public required UserManager UserManager { get; set; }
    [Inject] public required IJSRuntime Js { get; set; }
    [SupplyParameterFromQuery] public string? Connection { get; set; }
    private string UserName = "";
    private string LibraryName = "";
    private ICollection<ShareInfo> SharedLibrariesWith = [];
    private string NewShare = "";
    private string? ShareMessage;
    private ShareInfo? Candidate;
    private bool IsBusy;
    protected override async Task OnInitializedAsync()
    {
        var user = await CurrentUserProvider.GetCurrentUserRequiredAsync();
        UserName = user.Name ?? user.UserName ?? ""; LibraryName = user.UsersLibraryName;
        ShareMessage = Connection == "connected" ? Ux.Text("LibrariesConnected") : Connection == "disconnected" ? Ux.Text("LibrariesDisconnected") : null;
        await LoadSharingInfo();
    }
    private async Task LoadSharingInfo()
    {
        var user = await CurrentUserProvider.GetCurrentUserRequiredAsync();
        user = await UserManager.GetUserAsync(user.Id) ?? user;
        var users = await UserManager.MathUserByLibraryCodeAsync(user.SharedLibraries);
        SharedLibrariesWith = users.Select(user => new ShareInfo(user.Id, user.Name ?? user.UserName ?? "", user.UsersLibraryName)).ToList();
    }
    private void CodeChanged(ChangeEventArgs e) { NewShare = e.Value?.ToString() ?? ""; Candidate = null; ShareMessage = null; }
    private async Task PreviewShare()
    {
        if (IsBusy) return;
        IsBusy = true; Candidate = null; ShareMessage = null;
        try
        {
            var code = NewShare.Trim();
            if (code == LibraryName) { ShareMessage = Ux.Text("OwnLibraryCode"); return; }
            if (SharedLibrariesWith.Any(share => share.LibraryCode == code)) { ShareMessage = Ux.Text("AlreadyConnected"); return; }
            var users = await UserManager.MathUserByLibraryCodeAsync([code]);
            var user = users.SingleOrDefault();
            if (user is null) { ShareMessage = Ux.Text("CodeNotFound"); return; }
            Candidate = new(user.Id, user.Name ?? user.UserName ?? "", user.UsersLibraryName);
        }
        catch (Exception) { ShareMessage = Ux.Text("ShareLookupFailed"); }
        finally { IsBusy = false; }
    }
    private async Task Connect()
    {
        if (Candidate is null || IsBusy) return;
        IsBusy = true;
        try
        {
            var current = await CurrentUserProvider.GetCurrentUserRequiredAsync();
            await UserManager.ShareLibraryAsync(current.Id, Candidate.LibraryCode);
            Candidate = null; NewShare = ""; ShareMessage = Ux.Text("LibrariesConnected"); await LoadSharingInfo();
            NavigationManager.NavigateTo("/api/session/refresh?connection=connected", forceLoad: true);
        }
        catch (Exception) { ShareMessage = Ux.Text("ShareFailed"); }
        finally { IsBusy = false; }
    }
    private async Task Disconnect(ShareInfo share)
    {
        if (IsBusy || !await Js.InvokeAsync<bool>("confirm", Ux.Format("ConfirmDisconnect", share.UserName))) return;
        IsBusy = true;
        try
        {
            var current = await CurrentUserProvider.GetCurrentUserRequiredAsync();
            await UserManager.UnShareLibraryAsync(current.Id, share.LibraryCode);
            ShareMessage = Ux.Text("LibrariesDisconnected"); await LoadSharingInfo();
            NavigationManager.NavigateTo("/api/session/refresh?connection=disconnected", forceLoad: true);
        }
        catch (Exception) { ShareMessage = Ux.Text("DisconnectFailed"); }
        finally { IsBusy = false; }
    }
    private async Task CopyCode() => ShareMessage = await Js.InvokeAsync<bool>("DotCastUi.copy", LibraryName) ? Ux.Text("CodeCopied") : Ux.Text("CopyCodeManually");
}
