using System.Security.Claims;
using Shared.Infrastructure.AppUser;
using Shared.Infrastructure.AppUser.Identity;

namespace DotCast.Infrastructure.AppUser
{
    public class UserInfo : UserInfoBase
    {
        public static string UsersLibraryClaimName => GetClaimNameInternal("users-library-name");
        public string UsersLibraryName { get; set; } = null!;

        public static string SharedLibraryClaimName => GetClaimNameInternal("shared-library");

        public List<string> SharedLibraries { get; set; } = new();

        public IReadOnlyCollection<string> AvailableLibraries => SharedLibraries.Append(UsersLibraryName).ToList() ?? [UsersLibraryName];
        public bool IsAdmin => Roles.Any(t => t.Name == UserRoleManager.AdminRoleName);

        protected override string GetClaimName(string propertyName)
        {
            return GetClaimNameInternal(propertyName);
        }

        private static string GetClaimNameInternal(string propertyName)
        {
            return $"dotcast-{propertyName}";
        }

        public override ICollection<Claim> GetUserInfoClaims()
        {
            var claims = base.GetUserInfoClaims();

            claims.Add(new Claim(UsersLibraryClaimName, UsersLibraryName));

            foreach (var library in SharedLibraries)
            {
                claims.Add(new Claim(SharedLibraryClaimName, library));
            }

            return claims;
        }

        public override void LoadFromClaims(IUserRoleManager<UserRole> userRoleManager, ICollection<Claim> claims)
        {
            base.LoadFromClaims(userRoleManager, claims);
            UsersLibraryName = claims.First(claim => claim.Type == UsersLibraryClaimName).Value;

            SharedLibraries = claims
                .Where(claim => claim.Type == SharedLibraryClaimName)
                .Select(claim => claim.Value)
                .Except([UsersLibraryName]).ToList();
        }
    }
}
