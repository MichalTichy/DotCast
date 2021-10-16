using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotCast.FileManager.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole())
                .AddSingleton<IPodcastFileNameManager, PodcastFileNameManager>()
                .BuildServiceProvider();
            var renamer = serviceProvider.GetRequiredService<IPodcastFileNameManager>();

            var pathProvided = args.Length == 1;
            var target = pathProvided ? args[0] : Environment.CurrentDirectory;

            if (!pathProvided)
            {
                Console.WriteLine($"Press enter to run file name maintenance in {target}.");
                Console.ReadKey();
            }

            Console.WriteLine($"Running file maintenance in {target}.");
            renamer.RenameFilesToUrlFriendlyNames(target, null);
        }
    }
}
