using Microsoft.Extensions.Logging;
using DotCast.Infrastructure.Blazor.ClaimsManagement;
using System.Security.Claims;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentTenancyProvider;

namespace DotCast.Infrastructure.CurrentUserProvider.Blazor;

public class BlazorUserInfoProvider<TUser, TRole>(IUserClaimsProvider claimsService, IUserRoleManager<TRole> userRoleManager) : ICurrentUserProvider<TUser>, ICurrentUserIdProvider
    where TUser : class, IAppUser<TRole>, new() where TRole : IUserRole
{
    public Task<TUser?> GetCurrentUserAsync()
    {
        var user = claimsService.User;


        if (user == null || user.Identity?.IsAuthenticated == false)
        {
            return Task.FromResult<TUser?>(null);
        }


        var appUser = GetFromClaims(user.Claims.ToArray());
        return Task.FromResult(appUser)!;
    }

    public async Task<TUser> GetCurrentUserRequiredAsync()
    {
        
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Current must be logged in!");
        }

        return currentUser;
    }

    private TUser GetFromClaims(Claim[] claims)
    {
        var user = new TUser();
        user.LoadFromClaims(userRoleManager, claims);
        return user;
    }

    public async Task<string?> GetCurrentUserIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.Id;
    }

    public async Task<string> GetCurrentUserIdRequiredAsync()
    {
        var user = await GetCurrentUserRequiredAsync();
        return user.Id;
    }
}
