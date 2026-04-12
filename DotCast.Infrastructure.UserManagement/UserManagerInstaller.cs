using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DotCast.Infrastructure.AppUser.Identity;
using DotCast.Infrastructure.IoC;
using DotCast.Infrastructure.UserManagement.Abstractions;

namespace DotCast.Infrastructure.UserManagement;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserManager<TUserInfo>(this IServiceCollection services)
        where TUserInfo : UserInfoBase
    {
        services.AddScoped<IUserManager<TUserInfo>, UserManagerBase<TUserInfo>>();
        return services;
    }
}
