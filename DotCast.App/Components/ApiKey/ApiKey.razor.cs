using DotCast.App.Components.ApiKey.Texts;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Infrastructure.ApiKeys;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
namespace DotCast.App.Components.ApiKey;
public partial class ApiKey : IDisposable
{
    [Inject] public required IMessagePublisher Messenger { get; set; }
    [Inject] public required ICurrentUserProvider<UserInfo> Users { get; set; }
    [Inject] public required IReadOnlyRepository<AccountApiKey> Keys { get; set; }
    [Inject] public required ILogger<ApiKey> Logger { get; set; }
    private readonly CancellationTokenSource lifetime = new();
    protected bool Busy { get; set; } = true;
    protected string? Credential { get; set; }
    protected string? Message { get; set; }
    protected ApiKeyInfo? Info { get; set; }
    protected override Task OnInitializedAsync() => RunAsync(RefreshAsync);
    private async Task RefreshAsync()
    {
        var owner = await Users.GetCurrentUserRequiredAsync();
        Info = await Keys.GetBySpecAsync(new AccountApiKeyInfoSpecification(owner.Id), lifetime.Token);
    }
    protected Task GenerateAsync() => RunAsync(async () => {
        Credential = null;
        Credential = await Messenger.RequestAsync<GenerateApiKey, string>(new(), lifetime.Token);
        await RefreshAsync();
    });
    protected Task RevokeAsync() => RunAsync(async () => {
        await Messenger.ExecuteAsync(new RevokeApiKey(), lifetime.Token);
        Credential = null;
        await RefreshAsync();
    });
    protected void DismissCredential() => Credential = null;
    private async Task RunAsync(Func<Task> operation)
    {
        Busy = true; Message = null;
        try { await operation(); }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        catch (Exception exception) { Logger.LogError(exception, "API key management failed"); Message = ApiKeyTexts.Get("Failed"); }
        finally { Busy = false; }
    }
    public void Dispose()
    {
        Credential = null;
        lifetime.Cancel();
        lifetime.Dispose();
    }
}
