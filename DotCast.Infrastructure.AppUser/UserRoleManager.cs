using Shared.Infrastructure.AppUser;
using Shared.Infrastructure.AppUser.Identity;

namespace DotCast.Infrastructure.AppUser
{
    public class UserRoleManager() : UserRoleManagerBase<UserRole>(new List<UserRole> { Admin, User })
    {
        public const string AdminRoleName = "Admin";
        public static readonly UserRole Admin = UserRole._Create(AdminRoleName, "A922DE8B-E530-47D8-A2B0-A573300A1FED");

        public const string UserRoleName = "User";
        public static readonly UserRole User = UserRole._Create(UserRoleName, "3DC6F646-632A-4FCE-B4DB-BF2746F8770A");
    }
}