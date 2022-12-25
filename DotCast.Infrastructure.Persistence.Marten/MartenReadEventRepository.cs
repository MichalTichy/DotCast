using System.Threading;
using System.Threading.Tasks;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class MartenReadEventRepository<T> : IReadEventRepository<T> where T : notnull
    {
        private readonly ISessionFactoryWithAlternateTenantSettings sessionFactory;

        public MartenReadEventRepository(ISessionFactoryWithAlternateTenantSettings sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await sessionFactory.QuerySessionAsync(tenantId);

            return await session.LoadAsync<T>(id, cancellationToken);
        }
    }
}