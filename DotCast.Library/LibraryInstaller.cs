using DotCast.Infrastructure.AppUser;
using DotCast.Library.RSS;
using DotCast.Library.Storage;
using DotCast.SharedKernel.Models;
using JasperFx.Core.Reflection;
using Marten.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.CurrentUserProvider;
using Shared.Infrastructure.IoC;
using Shared.Infrastructure.Persistence.Marten.Repository.Document;
using Shared.Infrastructure.Persistence.Marten.SessionFactory;
using Shared.Infrastructure.Persistence.Marten.StorageConfiguration;
using Shared.Infrastructure.Persistence.Repositories;

namespace DotCast.Library
{
    public class AudioBookRepository(ISessionFactoryWithAlternateTenantSettings sessionFactory, ICurrentUserProvider<UserInfo> userProvider) : MartenRepository<AudioBook>(sessionFactory)
    {
        public override async Task<IMartenQueryable<AudioBook>> PreprocessQueryAsync(IMartenQueryable<AudioBook> queryable)
        {
            var query = await base.PreprocessQueryAsync(queryable);
            var user = await userProvider.GetCurrentUserRequiredAsync();

            return query.Where(x => user.AvailableLibraries.Contains(x.LibraryId)).As<IMartenQueryable<AudioBook>>();
        }

        public override async Task<AudioBook?> GetByIdAsync(string id, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            var book = await base.GetByIdAsync(id, cancellationToken, tenantId);

            if (book != null)
            {
                var user = await userProvider.GetCurrentUserRequiredAsync();
                if (!user.AvailableLibraries.Contains(book.LibraryId))
                {
                    return null;
                }
            }

            return book;
        }
    }

    public class LibraryInstaller : ILowPriorityInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddTransient<IStorageConfiguration, AudioBookStorageConfiguration>();
            services.AddTransient<ILibraryApiInformationProvider, LibraryApiInformationProvider>();
            services.AddSingleton<AudioBookRssGenerator>();

            services.AddScoped<IRepository<AudioBook>, AudioBookRepository>();
            services.AddScoped<IReadOnlyRepository<AudioBook>, AudioBookRepository>();
        }
    }
}