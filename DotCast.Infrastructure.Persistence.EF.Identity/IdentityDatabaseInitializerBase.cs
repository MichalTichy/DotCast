using DotCast.Infrastructure.Initializer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.AppUser.Identity;

namespace DotCast.Infrastructure.Persistence.EF.Identity;

public abstract class IdentityDatabaseInitializerBase<T>(AspNetIdentityDbContextBase<T> dbContext, IRoleStore<UserRole> roleStore, IUserRoleManager<UserRole> userRoleManager)
    : InitializerBase where T : UserInfoBase
{
    public override int Priority => int.MaxValue;
    public override InitializerTrigger Trigger => InitializerTrigger.OnStartup;
    public override bool RunOnlyInLeaderInstance => true;

    protected override async Task RunInitializationLogicAsync()
    {
        await dbContext.Database.EnsureCreatedAsync();

        foreach (var userRole in userRoleManager.AllRoles)
        {
            var existingRole = await roleStore.FindByNameAsync(userRole.NormalizedName!, CancellationToken.None);
            if (existingRole != null)
            {
                continue;
            }

            await roleStore.CreateAsync(userRole, CancellationToken.None);
        }
    }
}
