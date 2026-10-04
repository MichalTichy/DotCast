using DotCast.App.Components.PersonalApiTokens.Texts;
using DotCast.Infrastructure.PersonalApiTokens.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace DotCast.App.Components.PersonalApiTokens;
public partial class PersonalApiTokens : IAsyncDisposable
{
    [Inject] public required IJSRuntime Js { get; set; }
    private IJSObjectReference? module;
    protected string Name { get; set; } = string.Empty;
    protected DateTime Expiry { get; set; } = DateTime.Today.AddDays(90);
    protected bool CanWrite { get; set; }
    protected bool Busy { get; set; } = true;
    protected string? Credential { get; set; }
    protected string? Message { get; set; }
    protected IReadOnlyList<PersonalTokenInfo> Tokens { get; set; } = [];
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await RunAsync(async () => {
            module = await Js.InvokeAsync<IJSObjectReference>("import", "./Components/PersonalApiTokens/PersonalApiTokens.razor.js");
            Tokens = await module.InvokeAsync<PersonalTokenInfo[]>("list");
        });
    }
    protected Task CreateAsync() => RunAsync(async () => {
        Credential = null;
        if (string.IsNullOrWhiteSpace(Name) || Expiry.Date <= DateTime.Today || Expiry.Date > DateTime.Today.AddDays(365))
        { Message = TokenUiTexts.Get("Invalid"); return; }
        Credential = await module!.InvokeAsync<string>("create", Name, Expiry.Date.ToUniversalTime(), CanWrite);
        Name = string.Empty;
        Tokens = await module!.InvokeAsync<PersonalTokenInfo[]>("list");
    });
    protected Task RevokeAsync(string id) => RunAsync(async () => {
        await module!.InvokeVoidAsync("revoke", id);
        Tokens = await module!.InvokeAsync<PersonalTokenInfo[]>("list");
    });
    protected void DismissCredential() => Credential = null;
    protected static string Status(PersonalTokenInfo token) => token.RevokedAt is not null ? TokenUiTexts.Get("Revoked") :
        token.ExpiresAt <= DateTimeOffset.UtcNow ? TokenUiTexts.Get("Expired") : TokenUiTexts.Get("Active");
    private async Task RunAsync(Func<Task> operation)
    {
        Busy = true; Message = null;
        try { await operation(); }
        catch (JSException) { Message = TokenUiTexts.Get("Failed"); }
        finally { Busy = false; await InvokeAsync(StateHasChanged); }
    }
    public async ValueTask DisposeAsync()
    {
        Credential = null;
        if (module is null) return;
        try { await module.DisposeAsync(); } catch (JSDisconnectedException) { }
    }
}
