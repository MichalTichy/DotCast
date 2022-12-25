using System.Threading.Tasks;
using Marten;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public interface ISessionFactoryWithAlternateTenantSettings : IAsyncSessionFactory
    {
        Task<IQuerySession> QuerySessionAsync(string? alternateTenantId);
        Task<IDocumentSession> OpenSessionAsync(string? alternateTenantId);
    }
}