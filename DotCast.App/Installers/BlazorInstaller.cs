using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Shared.Infrastructure.Blazor.ClaimsManagement;
using Shared.Infrastructure.IoC;

namespace DotCast.App.Installers
{
    public class BlazorInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<CircuitHandler, UserCircuitHandler>();

            services
                .AddBlazorise(options => { options.Immediate = true; })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();
        }
    }
}
