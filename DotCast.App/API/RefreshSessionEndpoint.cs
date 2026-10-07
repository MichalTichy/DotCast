using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentUserProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DotCast.App.API;

[Authorize]
public class RefreshSessionEndpoint(ICurrentUserProvider<UserInfo> currentUserProvider,
    DotCast.Infrastructure.AppUser.UserManager userManager, SignInManager<UserInfo> signInManager) : ControllerBase
{
    [HttpGet("/api/session/refresh")]
    public async Task<IActionResult> Handle([FromQuery] string? connection)
    {
        var current = await currentUserProvider.GetCurrentUserRequiredAsync();
        var user = await userManager.GetUserAsync(current.Id);
        if (user is null) return Unauthorized();
        await signInManager.RefreshSignInAsync(user);
        var query = connection is "connected" or "disconnected" ? $"?connection={connection}" : "";
        return LocalRedirect("/UserProfile" + query);
    }
}
