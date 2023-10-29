using DotCast.Infrastructure.Initializer;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAllInitializers(this IServiceCollection serviceCollection,
            string dllPrefix = "DotCast")
        {
            var type = typeof(IInitializer);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();


            var matchedAssemblies = assemblies.Where(t => t.GetName().Name?.StartsWith(dllPrefix) ?? true).ToArray();
            foreach (var assembly in matchedAssemblies)
            {
                var types = assembly.GetTypes();
                var initializerTypes = types.Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract).ToArray();
                foreach (var initializerType in initializerTypes)
                {
                    serviceCollection.AddTransient(typeof(IInitializer), initializerType);
                }
            }
        }
    }
}
