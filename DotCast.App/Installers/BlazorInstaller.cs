using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using DotCast.Infrastructure.IoC;

namespace DotCast.App.Installers
{
    public class BlazorInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();


            services
                .AddBlazorise(options => { options.Immediate = true; })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();
        }
    }
}
