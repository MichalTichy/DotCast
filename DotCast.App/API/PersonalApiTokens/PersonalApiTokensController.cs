using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.PersonalApiTokens.Models;
using DotCast.Infrastructure.PersonalApiTokens.UseCases;
using Microsoft.AspNetCore.Antiforgery;
using DotCast.Infrastructure.PersonalApiTokens.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DotCast.App.API.PersonalApiTokens;
[ApiController]
[Route("api/personal-tokens")]
[Authorize(AuthenticationSchemes = PersonalTokenDefaults.ManagementScheme)]
[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PersonalApiTokensController(IMessagePublisher messenger, IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> List(CancellationToken cancellationToken)
    {
        var tokens = await messenger.RequestAsync<ListPersonalTokens, IReadOnlyList<PersonalTokenInfo>>(new(), cancellationToken);
        return Ok(new { tokens, requestToken = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });
    }
    [HttpPost]
    public async Task<ActionResult<CreatedPersonalToken>> Create(CreatePersonalToken request, CancellationToken cancellationToken)
    {
        try { return Ok(await messenger.RequestAsync<CreatePersonalToken, CreatedPersonalToken>(request, cancellationToken)); }
        catch (ArgumentException) { return BadRequest(new { code = "invalid_input" }); }
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> Revoke(string id, CancellationToken cancellationToken)
    {
        try { await messenger.ExecuteAsync(new RevokePersonalToken(id), cancellationToken); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
