using DotCast.Infrastructure.AppUser;
using Microsoft.AspNetCore.Identity;
using Shared.Infrastructure.AppUser;
using Shared.Infrastructure.AppUser.Identity;
using Shared.Infrastructure.Persistence.EF.Identity;

namespace DotCast.Infrastructure.Persistence.EF.Identity
{
    public class EntityFrameworkIdentityDatabaseInitializer(AspNetCoreIdentityDbContext dbContext, IRoleStore<UserRole> roleStore, IUserRoleManager<UserRole> userRoleManager)
        : IdentityDatabaseInitializerBase<UserInfo>(dbContext, roleStore, userRoleManager);
}
