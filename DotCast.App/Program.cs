using DotCast.BookInfoProvider;
using DotCast.Infrastructure.Initializer;
using DotCast.Infrastructure.IoC;
using DotCast.Library;
using DotCast.SharedKernel.Messages;
using DotCast.Storage;
using DotCast.Storage.Processing;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Net;
using DotCast.Library.API;
using DotCast.Storage.API;
using Wolverine;

namespace DotCast.App
{
    public class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "ASP0014:Suggest using top level route registrations", Justification = "<Pending>")]
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var isProduction = IsProduction();

            InstallerDiscovery.RunInstallersFromAllReferencedAssemblies(builder.Services, builder.Configuration, isProduction);
            builder.Services.AddControllers()
                .AddApplicationPart(typeof(UploadFileEndpoint).Assembly)
                .AddApplicationPart(typeof(GetRssEndpoint).Assembly);
            builder.Services.AddAllInitializers();

            builder.Host.UseWolverine(options =>
            {
                options.Discovery.IncludeAssembly(typeof(LibraryInstaller).Assembly);
                options.Discovery.IncludeAssembly(typeof(StorageInstaller).Assembly);
                options.Discovery.IncludeAssembly(typeof(AudiobookInfoProviderInstaller).Assembly);
                options.Discovery.IncludeAssembly(typeof(ProcessingInstaller).Assembly);

                options.Discovery.IncludeType(typeof(FileUploadTracker));
                options.Discovery.IncludeType(typeof(ProcessingPipeline));

                var describeHandlerMatch = options.DescribeHandlerMatch(typeof(ProcessingPipeline));
            });

            builder.Host.UseSystemd();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Use Developer Exception Page in Development
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios.
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
