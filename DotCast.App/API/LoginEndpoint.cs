using System.ComponentModel.DataAnnotations;
using System.Web;
using Ardalis.ApiEndpoints;
using DotCast.Infrastructure.AppUser;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.UIException;
using Shared.Infrastructure.UserManagement.Abstractions;

namespace DotCast.App.API
{
    public class LoginModel
    {
        [Required]
        public required string Username { get; init; }

        [Required]
        public required string Password { get; init; }

        public bool RememberMe { get; init; }
    }

    public class LoginEndpoint(IUserManager<UserInfo> userManager) : EndpointBaseAsync.WithRequest<LoginModel>.WithActionResult<string>
    {
        public const string Address = "/api/login";

        [HttpPost(Address)]
        public override async Task<ActionResult<string>> HandleAsync(LoginModel request, CancellationToken cancellationToken = new())
        {
            try
            {
                var result = await userManager.LoginAsync(request.Username, request.Password, request.RememberMe);

                if (result)
                {
                    return Redirect("/");
                }

                await userManager.LogoutAsync();
                return Redirect($"/Login?Message={HttpUtility.UrlEncode("Login failed")}");
            }
            catch (UiException exception)
            {
                await userManager.LogoutAsync();
                return Redirect($"/Login?Message={HttpUtility.UrlEncode(exception.MessageForUser)}");
            }
        }
    }
}
