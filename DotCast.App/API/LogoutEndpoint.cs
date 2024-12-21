using Ardalis.ApiEndpoints;
using DotCast.Infrastructure.AppUser;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.UserManagement.Abstractions;

namespace DotCast.App.API
{
    public class LogoutEndpoint(IUserManager<UserInfo> userManager) : EndpointBaseAsync.WithoutRequest.WithActionResult
    {
        public const string Address = "/api/logout";

        [HttpGet(Address)]
        public override async Task<ActionResult> HandleAsync(CancellationToken cancellationToken = new())
        {
            await userManager.LogoutAsync();
            return Redirect("/Login");
        }
    }
}