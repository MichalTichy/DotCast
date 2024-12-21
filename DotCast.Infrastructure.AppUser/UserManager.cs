using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Shared.Infrastructure.AppUser;
using Shared.Infrastructure.AppUser.Identity;
using Shared.Infrastructure.UserManagement;

namespace DotCast.Infrastructure.AppUser
{
    public class UserManager(UserManager<UserInfo> internalUserManager, SignInManager<UserInfo> signInManager, IUserRoleManager<UserRole> userRoleManager)
        : UserManagerBase<UserInfo>(internalUserManager, signInManager, userRoleManager)
    {
        public async Task<ICollection<UserInfo>> MathUserByLibraryCodeAsync(ICollection<string> libraryCodes)
        {
            var users = new List<UserInfo>();
            foreach (var libraryCode in libraryCodes)
            {
                var user = await GetUserByLibraryCodeAsync(libraryCode);
                if (user != null)
                {
                    users.Add(user);
                }
            }

            return users;
        }

        private async Task<UserInfo?> GetUserByLibraryCodeAsync(string libraryCode)
        {
            return (await InternalUserManager.GetUsersForClaimAsync(new Claim(UserInfo.UsersLibraryClaimName, libraryCode))).SingleOrDefault();
        }

        public async Task ShareLibraryAsync(string targetUserId, string sourceUserLibraryCode)
        {
            var targetUser = await InternalUserManager.FindByIdAsync(targetUserId);
            if (targetUser == null)
            {
                throw new ArgumentException("Target user not found!", targetUserId);
            }

            var sourceUser = await GetUserByLibraryCodeAsync(sourceUserLibraryCode);
            if (sourceUser == null)
            {
                throw new ArgumentException("Source user not found!", targetUserId);
            }

            await AddAddLibraryCodeToSharedLibrariesAsync(sourceUserLibraryCode, targetUser);
            await AddAddLibraryCodeToSharedLibrariesAsync(targetUser.UsersLibraryName, sourceUser);
        }

        public async Task UnShareLibraryAsync(string targetUserId, string sourceUserLibraryCode)
        {
            var targetUser = await InternalUserManager.FindByIdAsync(targetUserId);
            if (targetUser == null)
            {
                throw new ArgumentException("Target user not found!", targetUserId);
            }

            var sourceUser = await GetUserByLibraryCodeAsync(sourceUserLibraryCode);
            if (sourceUser == null)
            {
                throw new ArgumentException("Source user not found!", targetUserId);
            }

            await RemoveLibraryCodeFromSharedLibrariesAsync(sourceUserLibraryCode, targetUser);
            await RemoveLibraryCodeFromSharedLibrariesAsync(targetUser.UsersLibraryName, sourceUser);
        }

        private async Task AddAddLibraryCodeToSharedLibrariesAsync(string libraryCode, UserInfo user)
        {
            if (user.SharedLibraries.Contains(libraryCode))
            {
                return;
            }

            user.SharedLibraries.Add(libraryCode);
            await UpdateClaimsAsync(user);
        }


        private async Task RemoveLibraryCodeFromSharedLibrariesAsync(string libraryCode, UserInfo user)
        {
            if (!user.SharedLibraries.Contains(libraryCode))
            {
                return;
            }

            user.SharedLibraries.Remove(libraryCode);
            await UpdateClaimsAsync(user);
        }
    }
}