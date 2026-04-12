using Marten;

namespace DotCast.Infrastructure.Persistence.Marten.SessionFactory;

public interface IAsyncSessionFactory
{
    Task<IQuerySession> QuerySessionAsync(UnitOfWorks.UnitOfWork uow);
    Task<IDocumentSession> OpenSessionAsync(UnitOfWorks.UnitOfWork uow);
}
