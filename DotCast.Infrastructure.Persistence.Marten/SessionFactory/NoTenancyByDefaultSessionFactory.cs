using Marten;
using DotCast.Infrastructure.CurrentTenancyProvider;

namespace DotCast.Infrastructure.Persistence.Marten.SessionFactory;

public class NoTenancyByDefaultSessionFactory(IDocumentStore store, ICurrentTenancyProvider currentTenancyProvider)
    : SessionFactoryWithAlternateTenantSettings(store, currentTenancyProvider), INoTenancyByDefaultSessionFactory
{
    public override async Task<IDocumentSession> OpenSessionAsync(string? alternateTenantId, UnitOfWorks.UnitOfWork uow)
    {
        if (alternateTenantId != null)
        {
            return await base.OpenSessionAsync(alternateTenantId, uow);
        }

        return await OpenSessionAsync(uow);
    }

    public override async Task<IQuerySession> QuerySessionAsync(string? alternateTenantId, UnitOfWorks.UnitOfWork uow)
    {
        if (alternateTenantId != null)
        {
            return await base.QuerySessionAsync(alternateTenantId, uow);
        }

        return await QuerySessionAsync(uow);
    }

    public override async Task<IDocumentSession> OpenSessionAsync(UnitOfWorks.UnitOfWork unitOfWork)
    {
        var sessionOptions = await CreateSessionOptionsAsync(null, unitOfWork);

        return await Store.OpenSerializableSessionAsync(sessionOptions);
    }

    public override async Task<IQuerySession> QuerySessionAsync(UnitOfWorks.UnitOfWork unitOfWork)
    {
        var sessionOptions = await CreateSessionOptionsAsync(null, unitOfWork);

        return Store.QuerySession(sessionOptions);
    }
}
