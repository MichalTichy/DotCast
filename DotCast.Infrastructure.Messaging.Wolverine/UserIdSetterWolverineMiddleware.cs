using System.Security.Claims;
using DotCast.Infrastructure.AppUser;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Blazor.ClaimsManagement;
using Shared.Infrastructure.CurrentUserProvider;
using Shared.Infrastructure.UserManagement.Abstractions;
using Wolverine;

namespace DotCast.Infrastructure.Messaging.Wolverine
{
    public class UserIdSetterWolverineMiddleware
    {
        public async Task BeforeAsync(Envelope envelope, IServiceProvider serviceProvider)
        {
            var userId = envelope.TenantId;
            var userProvider = serviceProvider.GetRequiredService<IUserClaimsProvider>();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var userManager = serviceProvider.GetRequiredService<IUserManager<UserInfo>>();
                var user = await userManager.GetUserAsync(userId);

                userProvider.User = user!.GetClaimsIdentity();
            }
            else
            {
                userProvider.User = null;
            }

            envelope.TenantId = null;
        }
    }
}