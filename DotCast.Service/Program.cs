using System.Collections.Generic;
using System.Linq;
using DotCast.Service.Auth;
using DotCast.Service.Controllers;
using DotCast.Service.PodcastProviders;
using DotCast.Service.Services;
using DotCast.Service.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DotCast.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.Configure<PodcastProviderSettings>(configuration.GetSection(nameof(PodcastProviderSettings)));
                    services.AddTransient(provider => provider.GetService< IOptions<PodcastProviderSettings>>().Value);

                    services.AddSingleton<IPodcastProvider, LocalPodcastProvider>();


                    services.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));
                    services.AddTransient(provider => provider.GetService<IOptions<AuthenticationSettings>>().Value);

                    services.AddAuthentication("BasicAuthentication")
                        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
                    services.AddSingleton<IAuthenticationManager,SimpleAuthenticationManager>();


                    services.AddHostedService<DotCastFileManager>();

                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.ConfigureServices(collection =>
                    {
                        collection.AddControllers();
                    });

                    builder.Configure(applicationBuilder =>
                    {
                        applicationBuilder.UseRouting();


                        applicationBuilder.UseAuthentication();
                        applicationBuilder.UseAuthorization();

                        applicationBuilder.UseEndpoints(routeBuilder =>
                        {
                            routeBuilder.MapControllers();
                        });

                    });
                });
    }
}
