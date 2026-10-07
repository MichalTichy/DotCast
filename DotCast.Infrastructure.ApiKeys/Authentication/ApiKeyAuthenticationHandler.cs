using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using DotCast.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace DotCast.Infrastructure.ApiKeys;
public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
    IReadOnlyRepository<AccountApiKey> keys, IApiKeyOwnerResolver owners)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var values)) return AuthenticateResult.NoResult();
        if (values.Count != 1 || !AuthenticationHeaderValue.TryParse(values[0], out var header) ||
            !string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) ||
            !ApiKeyCredential.TryHash(header.Parameter, out var hash))
            return AuthenticateResult.Fail("Invalid API key.");
        var ownerId = await keys.GetBySpecAsync(new FindApiKeyOwnerSpecification(hash), Context.RequestAborted);
        var user = ownerId is null ? null : await owners.FindAsync(ownerId, Context.RequestAborted);
        if (user is null) return AuthenticateResult.Fail("Invalid API key.");
        var principal = user.GetClaimsIdentity();
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(ApiKeyDefaults.KeyIdClaim, hash));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers.WWWAuthenticate = "Bearer";
        return Task.CompletedTask;
    }
}
