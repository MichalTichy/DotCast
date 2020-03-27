using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Microsoft.Owin.Hosting;

namespace DotCast
{
    class Program
    {

        static void Main(string[] args)
        {
            LoadConfiguration();
            using (WebApp.Start($"http://*:{AppConfiguration.Port}"))
            {
                Console.WriteLine("Podcast server started");
                while (true)
                {
                    RenameFilesToUrlFriendlyNames();
                    Thread.Sleep(TimeSpan.FromMinutes(5));
                }
            }
        }

        private static void RenameFilesToUrlFriendlyNames()
        {
            var rootDirectory = new DirectoryInfo(AppConfiguration.PodcastsLocation);
            foreach (var directory in rootDirectory.GetDirectories())
            {
                var newDirectoryName = TransformToUrlSafeFileName(directory.Name);
                if (directory.Name != newDirectoryName)
                {
                    directory.MoveTo(Path.Combine(directory?.Parent.FullName ?? "", newDirectoryName));
                }

                foreach (var file in directory.EnumerateFiles())
                {
                    var fileName = file.Name;
                    var newFileName = TransformToUrlSafeFileName(fileName);
                    if (fileName != newFileName)
                    {
                        file.MoveTo(Path.Combine(directory.FullName, newFileName));
                    }
                }
            }
        }
        static string TransformToUrlSafeFileName(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace(' ', '_');
        }

        private static void LoadConfiguration()
        {
            AppConfiguration.Port = ConfigurationManager.AppSettings["Port"];
            AppConfiguration.PodcastsLocation = ConfigurationManager.AppSettings["PodcastsLocation"];
        }
    }
}
