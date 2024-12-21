using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.IoC;
using Shared.Infrastructure.UserManagement.Abstractions;

namespace DotCast.Infrastructure.AppUser
{
    public class UserManagerInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddScoped<IUserManager<UserInfo>, UserManager>();
            services.AddScoped<UserManager>();
        }
    }
}