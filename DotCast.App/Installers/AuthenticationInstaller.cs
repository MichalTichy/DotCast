using DotCast.App.Auth;
using DotCast.Infrastructure.IoC;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Options;

namespace DotCast.App.Installers
{
    public class AuthenticationInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddTransient(provider => provider.GetRequiredService<IOptions<AuthenticationSettings>>().Value);

            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            services.AddSingleton<IAuthenticationManager, SimpleAuthenticationManager>();
            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

            services.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));
        }
    }
}