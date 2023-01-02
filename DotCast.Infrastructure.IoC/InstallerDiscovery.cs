using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotCast.Infrastructure.IoC
{
    public static class InstallerDiscovery
    {
        public static void RunInstallersFromAllReferencedAssemblies(IServiceCollection serviceCollection,
            IConfiguration configuration,
            bool isProduction,
            bool forceLoadAssemblies = true,
            string dllPrefix = "DotCast")
        {
            if (forceLoadAssemblies)
            {
                ForceLoadAssemblies(dllPrefix);
            }

            var type = typeof(IInstaller);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var highPriorityInstallers = new List<IInstaller>();
            var normalPriorityInstallers = new List<IInstaller>();
            var lowPriorityInstallers = new List<IInstaller>();

            foreach (var assembly in assemblies.Where(t => t.GetName().Name?.StartsWith(dllPrefix) ?? true))
            {
                foreach (var installerType in assembly.GetTypes().Where(p => type.GetTypeInfo().IsAssignableFrom(p) && p.IsClass && !p.IsAbstract))
                {
                    if (Activator.CreateInstance(installerType) is not IInstaller installer)
                    {
                        continue;
                    }

                    if (installer is IHighPriorityInstaller)
                    {
                        highPriorityInstallers.Add(installer);
                    }
                    else if (installer is ILowPriorityInstaller)
                    {
                        lowPriorityInstallers.Add(installer);
                    }
                    else
                    {
                        normalPriorityInstallers.Add(installer);
                    }
                }
            }


            foreach (var installer in highPriorityInstallers)
            {
                installer.Install(serviceCollection, configuration, isProduction);
            }


            foreach (var installer in normalPriorityInstallers)
            {
                installer.Install(serviceCollection, configuration, isProduction);
            }


            foreach (var installer in lowPriorityInstallers)
            {
                installer.Install(serviceCollection, configuration, isProduction);
            }
        }


        private static void ForceLoadAssemblies(string? startsWith)
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

            string[] referencedPaths;
            if (string.IsNullOrWhiteSpace(startsWith))
            {
                referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            }
            else
            {
                referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, $"{startsWith}*.dll");
            }

            var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

            toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));
        }
    }
}