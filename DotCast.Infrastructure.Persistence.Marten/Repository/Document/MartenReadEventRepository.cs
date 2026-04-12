using DotCast.Infrastructure.Persistence.Marten.Repository.Events;
using DotCast.Infrastructure.Persistence.Marten.SessionFactory;
using DotCast.Infrastructure.Persistence.Marten.UnitOfWorks;

namespace DotCast.Infrastructure.Persistence.Marten.Repository.Document;

public class MartenReadEventRepository<T>(ISessionFactoryWithAlternateTenantSettings sessionFactory) : IReadEventRepository<T>
    where T : class
{
    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default, string? tenantId = null)
    {
        await using var uow = new UnitOfWorkProvider();
        await using var session = await sessionFactory.QuerySessionAsync(tenantId, uow.Get());

        var result = await session.LoadAsync<T>(id, cancellationToken);

        await uow.CommitAsync();
        return result;
    }
}
