using System.Security.Claims;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentUserProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
namespace DotCast.Infrastructure.PersonalApiTokens.UseCases;
public sealed class TokenManagementAccess(IHttpContextAccessor accessor, ICurrentUserProvider<UserInfo> users)
{
    public async Task<string> RequireOwnerAsync()
    {
        // Token-management HTTP requests require the application's Identity cookie.
        var http = accessor.HttpContext ?? throw new UnauthorizedAccessException();
        if (http.Features.Get<Authentication.PersonalTokenIdentity>() is not null)
            throw new UnauthorizedAccessException();
        var cookie = await http.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        var user = await users.GetCurrentUserRequiredAsync();
        if (!cookie.Succeeded || cookie.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) != user.Id)
            throw new UnauthorizedAccessException();
        return user.Id;
    }
}
