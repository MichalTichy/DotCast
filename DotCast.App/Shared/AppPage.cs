using DotCast.Infrastructure.AppUser;
using Microsoft.AspNetCore.Authorization;

namespace DotCast.App.Shared
{
    [Authorize(Roles = UserRoleManager.UserRoleName)]
    public class AppPage : AppComponentBase
    {
    }
}