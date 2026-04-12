using Marten;
using DotCast.Infrastructure.CurrentTenancyProvider;

namespace DotCast.Infrastructure.Persistence.Marten.SessionFactory;

public class SessionFactoryWithAlternateTenantSettings(IDocumentStore store, ICurrentTenancyProvider currentTenancyProvider)
    : AppMartenSessionFactory(store, currentTenancyProvider), ISessionFactoryWithAlternateTenantSettings
{
    public virtual async Task<IQuerySession> QuerySessionAsync(string? alternateTenantId, UnitOfWorks.UnitOfWork uow)
    {
        var tenantId = alternateTenantId ?? await CurrentTenancyProvider.GetUserTenantAsync();
        var sessionOptions = await CreateSessionOptionsAsync(tenantId, uow);

        return Store.QuerySession(sessionOptions);
    }

    public virtual async Task<IDocumentSession> OpenSessionAsync(string? alternateTenantId, UnitOfWorks.UnitOfWork uow)
    {
        var tenantId = alternateTenantId ?? await CurrentTenancyProvider.GetUserTenantAsync();
        var sessionOptions = await CreateSessionOptionsAsync(tenantId, uow);

        return await Store.OpenSerializableSessionAsync(sessionOptions);
    }
}
