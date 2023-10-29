using System.Threading.Tasks;
using Marten;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public interface IAsyncSessionFactory
    {
        Task<IQuerySession> QuerySessionAsync();
        Task<IDocumentSession> OpenSessionAsync();
    }
}
