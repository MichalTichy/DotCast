using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace DotCast.Infrastructure.Blazor.ClaimsManagement;

public sealed class UserCircuitHandler(AuthenticationStateProvider authenticationStateProvider,
        IUserClaimsProvider userService,
        ILogger<UserCircuitHandler> logger)
    : CircuitHandler, IDisposable
{
    public override int Order => 1;

    public void Dispose()
    {
        authenticationStateProvider.AuthenticationStateChanged -=
            AuthenticationChanged;
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit,
        CancellationToken cancellationToken)
    {
        authenticationStateProvider.AuthenticationStateChanged +=
            AuthenticationChanged;
        var authState = authenticationStateProvider.GetAuthenticationStateAsync();
        await UpdateAuthenticationAsync(authState);
        await base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    async Task UpdateAuthenticationAsync(Task<AuthenticationState> task)
    {
        try
        {
            var state = await task;
            userService.User = state.User;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update authentication info on authentication change.");
        }
    }
    private void AuthenticationChanged(Task<AuthenticationState> task)
    {
        _ = UpdateAuthenticationAsync(task);
    }

    public override async Task OnConnectionUpAsync(Circuit circuit,
        CancellationToken cancellationToken)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        userService.User = state.User;
    }
}
