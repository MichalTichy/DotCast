using Microsoft.AspNetCore.Identity;

namespace DotCast.Infrastructure.UserManagement.Abstractions;

public interface IUserManager<T> where T : IdentityUser
{
    Task<T?> GetUserAsync(string userId);
    Task<T?> GetUserByEmailAsync(string userEmail);
    Task<string> NewUserAsync(T user, string? initialPassword = null);
    Task RemoveUserAsync(string user);
    Task UpdateUserAsync(string id, string email, string name, string? phone);
    Task SetInitialPasswordAsync(string userId, string initialPassword);
    Task<ICollection<T>> GetAllUsersAsync();
    Task<bool> LoginAsync(string userName, string password, bool remember);
    Task LogoutAsync();
    Task<string> GetPasswordResetTokenAsync(string userId);
    Task ResetPasswordAsync(string userId, string passwordToken, string newPassword);
    Task ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    Task EnsureUserHasRoleAsync(string userId, string roleName);
    Task TryRemoveUserRoleAsync(string userId, string roleName);
    Task RemoveAllUserRolesAsync(string userId);
    ICollection<string> GetPasswordErrors(string password);
    ICollection<string> GetPasswordErrors(T user, string password);
    Task UpdateUserAsync(T user);
    Task<T?> GetUserByUserNameAsync(string userName);
    Task UpdateClaimsAsync(T userInfo);
    Task FillInRolesAsync(T userInfo);
    string EncodeToken(string token);
    string DecodeToken(string token);
    Task LockoutUserAsync(string userName);
    Task EndLockoutForUserAsync(string userName);
}
