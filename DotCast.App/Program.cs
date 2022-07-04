using System.Net;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using DotCast.App.Auth;
using DotCast.PodcastProvider.Base;
using DotCast.PodcastProvider.FileSystem;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace DotCast.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();


            builder.Services.AddTransient(provider => provider.GetRequiredService<IOptions<AuthenticationSettings>>().Value);

            builder.Services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            builder.Services.AddSingleton<IAuthenticationManager, SimpleAuthenticationManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

            builder.Services
                .AddBlazorise(options => { options.Immediate = true; })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

            builder.Services.InstallApp(builder.Configuration);


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            var fileSystemOptions = app.Services.GetRequiredService<IOptions<FileSystemPodcastProviderOptions>>().Value;
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(fileSystemOptions.PodcastsLocation, Directory.GetCurrentDirectory())),
                RequestPath = "/files",
                OnPrepareResponse = async ctx =>
                {
                    if (ctx.File.Name.EndsWith(".png") || ctx.File.Name.EndsWith(".jpg"))
                    {
                        return;
                    }

                    var result = await ctx.Context.AuthenticateAsync();
                    if (!result.Succeeded)
                    {
                        await ctx.Context.ChallengeAsync();
                        ctx.Context.Response.StatusCode = 401;

                        ctx.Context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        ctx.Context.Response.ContentLength = 0;
                        ctx.Context.Response.Body = Stream.Null;
                    }
                }
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(fileSystemOptions.ZippedPodcastsLocation, Directory.GetCurrentDirectory())),
                RequestPath = "/zip",
                OnPrepareResponse = async ctx =>
                {
                    var result = await ctx.Context.AuthenticateAsync();
                    if (!result.Succeeded)
                    {
                        await ctx.Context.ChallengeAsync();
                        ctx.Context.Response.StatusCode = 401;

                        ctx.Context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        ctx.Context.Response.ContentLength = 0;
                        ctx.Context.Response.Body = Stream.Null;
                    }
                }
            });
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            var podcastInfoProvider = app.Services.GetRequiredService<IPodcastInfoProvider>();
            var podcastDownloader = app.Services.GetRequiredService<IPodcastDownloader>();

            var podcastInfos = podcastInfoProvider.GetPodcasts();

            _ = Parallel.ForEachAsync(podcastInfos, new ParallelOptions {MaxDegreeOfParallelism = 5}, async (podcastInfo, token) =>
            {
                logger.LogInformation("Ensuring that podcast {podcastName} has ZIP version.", podcastInfo.Id);
                await podcastDownloader.GenerateZip(podcastInfo.Id);
            });


            app.Run();
        }
    }
}