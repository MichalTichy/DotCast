using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotCast.App.Endpoints
{
    public class LoginEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult
    {
        [HttpGet("/login")]
        [AllowAnonymous]
        public override async Task<ActionResult> HandleAsync(CancellationToken cancellationToken = new())
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return Redirect("/");
            }

            await HttpContext.ChallengeAsync();
            return Unauthorized();
        }
    }
}
