using DotCast.Infrastructure.AppUser;
using DotCast.SharedKernel.Models;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.AppUser;
using Shared.Infrastructure.AppUser.Identity;
using Shared.Infrastructure.Initializer;
using Shared.Infrastructure.UserManagement.Abstractions;

namespace DotCast.App.Auth
{
    public class UserSeedInitializer(IUserManager<UserInfo> userManager, IOptions<AuthenticationSettings> options) : InitializerBase
    {
        public override int Priority => 0;
        public override InitializerTrigger Trigger => InitializerTrigger.OnStartup;
        public override bool RunOnlyInLeaderInstance => true;

        protected override async Task RunInitializationLogicAsync()
        {
            var usersToSeed = options.Value.Users;
            foreach (var userToSeed in usersToSeed)
            {
                var foundUser = await userManager.GetUserAsync(userToSeed.Username);
                if (foundUser != null)
                {
                    continue;
                }


                var libraryName = LibraryCode.Generate();
                var newUser = new UserInfo
                {
                    Id = userToSeed.Username,
                    Name = userToSeed.Username,
                    UserName = userToSeed.Username,
                    UsersLibraryName = libraryName,
                    Email = $"{userToSeed.Username}@dotcast.com",
                    Roles = new List<UserRole>
                    {
                        UserRoleManager.User
                    }
                };

                if (userToSeed.IsAdmin)
                {
                    newUser.Roles.Add(UserRoleManager.Admin);
                }

                await userManager.NewUserAsync(newUser, userToSeed.Password);
            }
        }
    }
}