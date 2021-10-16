using System.IO;
using System.Net;
using DotCast.FileManager;
using DotCast.Service.Auth;
using DotCast.Service.PodcastProviders;
using DotCast.Service.Services;
using DotCast.Service.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DotCast.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<PodcastProviderSettings>(Configuration.GetSection(nameof(PodcastProviderSettings)));
            services.AddTransient(provider => provider.GetService<IOptions<PodcastProviderSettings>>().Value);

            services.AddSingleton<IPodcastProvider, LocalPodcastProvider>();


            services.Configure<AuthenticationSettings>(Configuration.GetSection(nameof(AuthenticationSettings)));
            services.AddTransient(provider => provider.GetService<IOptions<AuthenticationSettings>>().Value);

            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            services.AddSingleton<IAuthenticationManager, SimpleAuthenticationManager>();

            services.AddSingleton<IPodcastFileNameManager, PodcastFileNameManager>();

            services.AddHostedService<DotCastFileManager>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var fileLocation = Configuration.GetSection("PodcastProviderSettings")["PodcastsLocation"];
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(fileLocation,Directory.GetCurrentDirectory())),
                RequestPath = "/files",
                OnPrepareResponse = async ctx =>
                {
                    var result = await ctx.Context.AuthenticateAsync();
                    if (!result.Succeeded)
                    {
                        await ctx.Context.ChallengeAsync();
                        ctx.Context.Response.StatusCode = 401;

                        ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        ctx.Context.Response.ContentLength = 0;
                        ctx.Context.Response.Body = Stream.Null;
                    }
                }
            });
        }
    }
}