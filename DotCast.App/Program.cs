using DotCast.BookInfoProvider;
using DotCast.Library;
using DotCast.SharedKernel.Messages;
using DotCast.Storage;
using DotCast.Storage.Processing;
using DotCast.App.Services;
using DotCast.Library.API;
using DotCast.Storage.API;
using Shared.Infrastructure.Initializer;
using Shared.Infrastructure.IoC;
using Wolverine;
using DotCast.Infrastructure.Messaging.Wolverine;
using Shared.Infrastructure.Blazor.ClaimsManagement;

namespace DotCast.App
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var appConfig = builder.Configuration.GetConnectionString("AppConfig");
            if (!string.IsNullOrWhiteSpace(appConfig))
            {
                builder.Configuration.AddAzureAppConfiguration(options =>
                {
                    var keyFilter = "dotcast:";
                    options.Connect(appConfig).Select($"{keyFilter}*").TrimKeyPrefix(keyFilter);
                });
            }

            var isProduction = IsProduction();

            InstallerDiscovery.RunInstallersFromAllReferencedAssemblies(builder.Services, builder.Configuration, isProduction, "DotCast");

            builder.Services.AddControllers()
                .AddApplicationPart(typeof(UploadFileEndpoint).Assembly)
                .AddApplicationPart(typeof(GetRssEndpoint).Assembly);
            builder.Services.AddAllInitializers("DotCast");

            builder.Host.UseWolverine(options =>
            {
                options.Policies.AddMiddleware<UserIdSetterWolverineMiddleware>();
                options.Discovery.IncludeAssembly(typeof(LibraryInstaller).Assembly);
                options.Discovery.IncludeAssembly(typeof(StorageInstaller).Assembly);
                options.Discovery.IncludeAssembly(typeof(AudiobookInfoProviderInstaller).Assembly);
                options.Discovery.IncludeAssembly(typeof(ProcessingInstaller).Assembly);

                options.Discovery.IncludeType(typeof(FileUploadTracker));
                options.Discovery.IncludeType(typeof(ProcessingPipeline));
                options.Discovery.IncludeType(typeof(ProcessingMonitor));

                options.LocalQueueFor<AudioBookReadyForProcessing>().MaximumParallelMessages(Environment.ProcessorCount / 2);
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
            app.UseMiddleware<UserClaimsMiddleware>();
            app.MapControllers();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            var initializerManager = app.Services.GetRequiredService<InitializerManager>();
            await initializerManager.RunAllInitializersAsync(InitializerTrigger.OnApplicationReady, InitializerTrigger.OnStartup);

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
