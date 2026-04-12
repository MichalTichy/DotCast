using Marten;

namespace DotCast.Infrastructure.Persistence.Marten.SessionFactory;

public interface ISessionFactoryWithAlternateTenantSettings : IAsyncSessionFactory
{
    Task<IQuerySession> QuerySessionAsync(string? alternateTenantId, UnitOfWorks.UnitOfWork uow);
    Task<IDocumentSession> OpenSessionAsync(string? alternateTenantId, UnitOfWorks.UnitOfWork uow);
}
