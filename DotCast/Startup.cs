using System.Collections;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace DotCast
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "PodcastApi",
                routeTemplate: "{controller}/{id}"
            );

            appBuilder.UseWebApi(config);

            appBuilder.UseStaticFiles(AppConfiguration.BasePath);
        }
    }
}
