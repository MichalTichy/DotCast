using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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
#if LINUX
                .UseSystemd()
#endif
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseUrls($"http://*:9876");
                });
    }
}
