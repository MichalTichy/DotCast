using Ardalis.GuardClauses;
using DotCast.App.Auth;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Persistence.EF.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.AppUser;
using Shared.Infrastructure.AppUser.Identity;
using Shared.Infrastructure.CurrentTenancyProvider;
using Shared.Infrastructure.CurrentUserProvider;
using Shared.Infrastructure.CurrentUserProvider.Blazor;
using Shared.Infrastructure.IoC;
using Shared.Infrastructure.Persistence.EF.Identity;
using UserInfo = DotCast.Infrastructure.AppUser.UserInfo;

namespace DotCast.App.Installers
{
    public class AuthenticationInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddTransient(provider => provider.GetRequiredService<IOptions<AuthenticationSettings>>().Value);

            services.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));

            var postgresConnectionString = configuration.GetConnectionString("DotCast");
            Guard.Against.NullOrWhiteSpace(postgresConnectionString, nameof(postgresConnectionString));

            services.AddIdentity<UserInfo, UserRole>(
                options =>
                {
                    options.Password.RequiredLength = 5;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredUniqueChars = 0;
                }).AddEFStores<AspNetCoreIdentityDbContext, UserInfo>().AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(o =>
                o.TokenLifespan = TimeSpan.FromDays(7));
            services.AddAuthorization();

            services.AddAuthentication(sharedOptions => { sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/Login";
                    options.AccessDeniedPath = "/Login";
                });

            var identityConnectionString = configuration.GetConnectionString("DotCast");
            Guard.Against.NullOrWhiteSpace(identityConnectionString);
            services.AddEFIdentity<AspNetCoreIdentityDbContext, UserInfo>(identityConnectionString);

            services.AddSingleton<IUserRoleManager<UserRole>, UserRoleManager>();
            services.AddScoped<ICurrentUserProvider<UserInfo>, BlazorUserInfoProvider<UserInfo, UserRole>>();
            services.AddScoped<ICurrentUserIdProvider, BlazorUserInfoProvider<UserInfo, UserRole>>();
        }
    }
}
