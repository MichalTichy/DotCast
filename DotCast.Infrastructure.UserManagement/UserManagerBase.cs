using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.AppUser.Identity;
using DotCast.Infrastructure.UIException;
using DotCast.Infrastructure.UserManagement.Abstractions;

namespace DotCast.Infrastructure.UserManagement;

public class UserManagerBase<T>(UserManager<T> internalUserManager, SignInManager<T> signInManager, IUserRoleManager<UserRole> userRoleManager) : IUserManager<T> where T : UserInfoBase
{
    protected readonly UserManager<T> InternalUserManager = internalUserManager;

    public virtual async Task UpdateUserAsync(T user)
    {
        var result = await InternalUserManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new UiException($"Nepodařilo se aktualizaovat uživatele. {string.Join(' ', result.Errors.Select(t => t.Description))}");
        }

        await UpdateClaimsAsync(user);
    }

    public virtual async Task<T?> GetUserAsync(string id)
    {
        var userInfo = await InternalUserManager.FindByIdAsync(id);

        if (userInfo != null)
        {
            await FillInRolesAsync(userInfo);
        }

        return userInfo;
    }

    public virtual async Task<T?> GetUserByUserNameAsync(string userName)
    {
        var userInfo = await InternalUserManager.FindByNameAsync(userName);

        if (userInfo != null)
        {
            await FillInRolesAsync(userInfo);
        }

        return userInfo;
    }

    public virtual async Task<T?> GetUserByEmailAsync(string userEmail)
    {
        var userInfo = await InternalUserManager.FindByEmailAsync(userEmail);
        if (userInfo != null)
        {
            await FillInRolesAsync(userInfo);
        }

        return userInfo;
    }

    public virtual async Task RemoveUserAsync(string userId)
    {
        var user = await GetUserAsync(userId)
                   ?? throw new ArgumentException("User not found.");
        await InternalUserManager.DeleteAsync(user);
    }

    public virtual async Task<string> NewUserAsync(T user, string? initialPassword = null)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new ArgumentException("Email is required.");
        }

        user.Id = Guid.NewGuid().ToString();
        user.UserName = user.UserName!;

        IdentityResult userInfo;

        if (string.IsNullOrWhiteSpace(initialPassword))
        {
            userInfo = await InternalUserManager.CreateAsync(user);
        }
        else
        {
            var errors = GetPasswordErrors(user, initialPassword);
            if (errors.Count != 0)
            {
                var error = new StringBuilder();
                foreach (var identityError in errors)
                {
                    error.AppendLine(identityError);
                }

                throw new UiException(error.ToString());
            }

            userInfo = await InternalUserManager.CreateAsync(user, initialPassword);
        }

        if (!userInfo.Succeeded)
        {
            var error = new StringBuilder();
            foreach (var identityError in userInfo.Errors)
            {
                error.AppendLine(identityError.Description);
            }

            throw new UiException(error.ToString());
        }

        await InternalUserManager.AddToRolesAsync(user, user.Roles.Select(t => t.Name!));

        await UpdateClaimsAsync(user);

        return user.Id;
    }

    public virtual async Task UpdateUserAsync(string id, string email, string name, string? phone)
    {
        var user = await GetUserAsync(id)
                   ?? throw new ArgumentException("User not found.", nameof(email));
        user.Email = email;
        user.PhoneNumber = phone;
        user.Name = name;

        await InternalUserManager.UpdateAsync(user);
        await UpdateClaimsAsync(user);
    }

    public virtual async Task SetInitialPasswordAsync(string userId, string initialPassword)
    {
        var user = await GetUserAsync(userId) ?? throw new ArgumentException("User not found.", nameof(userId));
        var result = await InternalUserManager.AddPasswordAsync(user, initialPassword);
        if (!result.Succeeded)
        {
            throw new UiException($"Nepodařilo se nastavit heslo. {string.Join(' ', result.Errors.Select(t => t.Description))}");
        }
    }

    public virtual async Task<ICollection<T>> GetAllUsersAsync()
    {
        var users = InternalUserManager.Users.ToList();
        foreach (var userInfo in users)
        {
            await FillInRolesAsync(userInfo);
        }

        return users;
    }

    public virtual async Task<bool> LoginAsync(string userName, string password, bool remember)
    {
        var result = await signInManager.PasswordSignInAsync(userName, password, remember, false);

        return result.Succeeded;
    }

    public async Task LogoutAsync()
    {
        await signInManager.SignOutAsync();
    }

    public async Task<string> GetPasswordResetTokenAsync(string userId)
    {
        var user = await GetUserAsync(userId) ?? throw new ArgumentException("User not found.", nameof(userId));
        var token = await InternalUserManager.GeneratePasswordResetTokenAsync(user);
        return EncodeToken(token);
    }

    public async Task ResetPasswordAsync(string userId, string passwordToken, string newPassword)
    {
        var user = await GetUserAsync(userId) ?? throw new ArgumentException("User not found.", nameof(userId));

        var result = await InternalUserManager.ResetPasswordAsync(user, DecodeToken(passwordToken), newPassword);
        if (!result.Succeeded)
        {
            throw new UiException($"Nepodařilo se nastavit heslo. {string.Join(' ', result.Errors.Select(t => t.Description))}");
        }
    }

    public virtual async Task ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var user = await GetUserAsync(userId) ?? throw new ArgumentException("User not found.", nameof(userId));
        var result = await InternalUserManager.ChangePasswordAsync(user, oldPassword, newPassword);
        if (!result.Succeeded)
        {
            throw new UiException($"Nepodařilo se změnit heslo. {string.Join(' ', result.Errors.Select(t => t.Description))}");
        }
    }

    public virtual async Task EnsureUserHasRoleAsync(string userId, string roleName)
    {
        var user = await GetUserAsync(userId)
                   ?? throw new ArgumentException("User not found.", nameof(userId));

        var role = userRoleManager.GetExistingRole(roleName)
                   ?? throw new ArgumentException("Role not found.", nameof(roleName));

        if (user.Roles.Contains(role))
            return;

        await InternalUserManager.AddToRoleAsync(user, roleName);
    }

    public virtual async Task TryRemoveUserRoleAsync(string userId, string roleName)
    {
        var role = userRoleManager.GetExistingRole(roleName)
                   ?? throw new ArgumentException("Role not found.", nameof(roleName));

        var user = await GetUserAsync(userId)
                   ?? throw new ArgumentException("User not found.", nameof(userId));

        if (!user.Roles.Contains(role))
        {
            return;
        }

        await InternalUserManager.RemoveFromRoleAsync(user, roleName);
    }

    public virtual async Task RemoveAllUserRolesAsync(string userId)
    {
        var user = await GetUserAsync(userId)
                   ?? throw new ArgumentException("User not found.", nameof(userId));

        await InternalUserManager.RemoveFromRolesAsync(user, user.Roles.Select(t => t.Name!));
    }

    public virtual ICollection<string> GetPasswordErrors(string password)
    {
        return GetPasswordErrors(null!, password);
    }

    public virtual ICollection<string> GetPasswordErrors(T user, string password)
    {
        //FIXME: this is used in a validation attribute which is wrong. The attributes should not handle long running opperations
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var errors = InternalUserManager.PasswordValidators
            .SelectMany(t => t.ValidateAsync(InternalUserManager, user!, password).Result.Errors.Select(e => e.Description)).ToList();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

        return errors;
    }

    public virtual async Task UpdateClaimsAsync(T userInfo)
    {
        var claims = userInfo.GetUserInfoClaims();
        var currentClaims = await InternalUserManager.GetClaimsAsync(userInfo);
        await InternalUserManager.RemoveClaimsAsync(userInfo, currentClaims);
        await InternalUserManager.AddClaimsAsync(userInfo, claims);
    }

    public virtual async Task FillInRolesAsync(T userInfo)
    {
        var roleNames = await InternalUserManager.GetRolesAsync(userInfo);
        userInfo.Roles = roleNames.Select(userRoleManager.GetExistingRole).ToList();
    }

    public virtual string EncodeToken(string token)
    {
        return Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(token));
    }

    public virtual string DecodeToken(string token)
    {
        var bytes = Base64UrlTextEncoder.Decode(token);
        return Encoding.UTF8.GetString(bytes);
    }

    public async Task LockoutUserAsync(string userName)
    {
        var userInfo = await InternalUserManager.FindByNameAsync(userName);
        if (userInfo is null)
        {
            throw new ArgumentException("User not found.", nameof(userName));
        }

        // Ensure lockout is enabled for this user, otherwise LockoutEnd is ignored.
        var enableResult = await InternalUserManager.SetLockoutEnabledAsync(userInfo, true);
        if (!enableResult.Succeeded)
        {
            throw new UiException($"Failed to enable lockout. {string.Join(' ', enableResult.Errors.Select(e => e.Description))}");
        }

        var lockoutResult = await InternalUserManager.SetLockoutEndDateAsync(userInfo, DateTimeOffset.UtcNow.AddYears(100));
        if (!lockoutResult.Succeeded)
        {
            throw new UiException($"Failed to lock out user. {string.Join(' ', lockoutResult.Errors.Select(e => e.Description))}");
        }
    }

    public async Task EndLockoutForUserAsync(string userName)
    {
        var userInfo = await InternalUserManager.FindByNameAsync(userName);
        if (userInfo is null)
        {
            throw new ArgumentException("User not found.", nameof(userName));
        }

        var result = await InternalUserManager.SetLockoutEndDateAsync(userInfo, null);
        if (!result.Succeeded)
        {
            throw new UiException($"Failed to end lockout. {string.Join(' ', result.Errors.Select(e => e.Description))}");
        }
    }
}