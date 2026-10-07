using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.UserManagement.Abstractions;
using Microsoft.AspNetCore.Identity;
namespace DotCast.Infrastructure.ApiKeys;
public sealed class ApiKeyOwnerResolver(IUserManager<UserInfo> users, UserManager<UserInfo> identityUsers) : IApiKeyOwnerResolver
{
    public async Task<UserInfo?> FindAsync(string ownerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.GetUserAsync(ownerId);
        if (user is null || await identityUsers.IsLockedOutAsync(user)) return null;
        cancellationToken.ThrowIfCancellationRequested();
        return user;
    }
}
