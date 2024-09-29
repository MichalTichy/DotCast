using Shared.Infrastructure.IoC;
using Shared.Infrastructure.Persistence.Marten.Extensions;

namespace DotCast.App.Installers
{
    public class DatabaseInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            var connectionString = configuration.GetConnectionString("DotCast") ?? throw new ArgumentException("Database connection string missing");
            services.AddNpgsqlDataSource(connectionString);
            services.AddMartenPostgresPersistence();
        }
    }
}
