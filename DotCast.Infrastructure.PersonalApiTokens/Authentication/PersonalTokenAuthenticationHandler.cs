using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using DotCast.Infrastructure.PersonalApiTokens.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace DotCast.Infrastructure.PersonalApiTokens.Authentication;
public sealed class PersonalTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
    IPersonalApiTokenStore tokens, ITokenOwnerResolver owners, TimeProvider time)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Context.Features.Set<PersonalTokenIdentity>(null);
        if (!Request.Headers.TryGetValue("Authorization", out var values)) return AuthenticateResult.NoResult();
        if (values.Count != 1 || !AuthenticationHeaderValue.TryParse(values[0], out var header) ||
            !string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) ||
            !PersonalTokenCredential.TryParse(header.Parameter, out var id, out var hash))
            return AuthenticateResult.Fail("Invalid API token.");
        var token = await tokens.FindAsync(id, Context.RequestAborted);
        if (token is null || !PersonalTokenCredential.IsValid(token, hash, time.GetUtcNow()))
            return AuthenticateResult.Fail("Invalid API token.");
        var user = await owners.FindAsync(token.OwnerId, Context.RequestAborted);
        if (user is null) return AuthenticateResult.Fail("Invalid API token.");
        Context.Features.Set(new PersonalTokenIdentity(user.Id, token.Id, token.CanWrite));
        return AuthenticateResult.Success(new AuthenticationTicket(user.GetClaimsIdentity(), Scheme.Name));
    }
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers.WWWAuthenticate = "Bearer";
        return Task.CompletedTask;
    }
}
