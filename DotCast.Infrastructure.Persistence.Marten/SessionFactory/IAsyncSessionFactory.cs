using System.Threading.Tasks;
using DotCast.Infrastructure.Persistence.Marten.UnitOfWorks;
using Marten;

namespace DotCast.Infrastructure.Persistence.Marten.SessionFactory
{
    public interface IAsyncSessionFactory
    {
        Task<IQuerySession> QuerySessionAsync(UnitOfWork uow);
        Task<IDocumentSession> OpenSessionAsync(UnitOfWork uow);
    }
}