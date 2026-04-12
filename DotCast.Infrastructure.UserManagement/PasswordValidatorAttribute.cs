using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using DotCast.Infrastructure.AppUser.Identity;
using DotCast.Infrastructure.UserManagement.Abstractions;

namespace DotCast.Infrastructure.UserManagement;

public class PasswordValidatorAttribute<T> : ValidationAttribute where T : UserInfoBase
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var password = value as string;
        if (string.IsNullOrEmpty(password))
        {
            return new ValidationResult("Password is required.");
        }

        // Get the UserManager service from the service provider
        var userManager = validationContext.GetRequiredService<IUserManager<T>>();

        var errors = userManager.GetPasswordErrors(password);

        if (errors.Count == 0)
        {
            return ValidationResult.Success!;
        }

        // Combine the errors from the IdentityResult
        var errorString = string.Join(", ", errors);
        return new ValidationResult(errorString);
    }
}