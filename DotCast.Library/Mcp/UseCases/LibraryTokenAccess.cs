using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.PersonalApiTokens.Authentication;
namespace DotCast.Library.Mcp.UseCases;
public sealed class LibraryTokenAccess(IPersonalTokenContext tokens, ICurrentUserProvider<UserInfo> users)
{
    public async Task<UserInfo> RequireAsync(string scope)
    {
        var identity = tokens.RequireScope(scope);
        var user = await users.GetCurrentUserRequiredAsync();
        if (user.Id != identity.OwnerId) throw new UnauthorizedAccessException("access_denied");
        return user;
    }
}
