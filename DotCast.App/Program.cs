using System.Net;
using DotCast.App.Middlewares;
using DotCast.Infrastructure.Initializer;
using DotCast.Infrastructure.IoC;
using DotCast.AudioBookProvider.FileSystem;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace DotCast.App
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var isProduction = IsProduction();

            InstallerDiscovery.RunInstallersFromAllReferencedAssemblies(builder.Services, builder.Configuration, isProduction);
            builder.Services.AddAllInitializers();

            builder.Host.UseSystemd();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<LegacyUrlRedirectMiddleware>();
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

            var fileSystemOptions = app.Services.GetRequiredService<IOptions<FileSystemAudioBookProviderOptions>>().Value;
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(fileSystemOptions.AudioBooksLocation, Directory.GetCurrentDirectory())),
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
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(fileSystemOptions.ZippedAudioBooksLocation, Directory.GetCurrentDirectory())),
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

            var initializerManager = app.Services.GetRequiredService<InitializerManager>();
            await initializerManager.RunAllInitializersAsync();

            await app.RunAsync();
        }

        private static bool IsProduction()
        {
            var isProduction = true;
#if DEBUG
            isProduction = false;
#endif
            return isProduction;
        }
    }
}
