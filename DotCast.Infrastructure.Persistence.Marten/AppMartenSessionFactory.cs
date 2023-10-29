using System.Threading.Tasks;
using Marten;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class AppMartenSessionFactory : IAsyncSessionFactory
    {
        protected readonly IDocumentStore Store;

        public AppMartenSessionFactory(IDocumentStore store)
        {
            Store = store;
        }

        public virtual Task<IQuerySession> QuerySessionAsync()
        {
            return Task.FromResult(Store.QuerySession());
        }

        public virtual Task<IDocumentSession> OpenSessionAsync()
        {
            return Task.FromResult(Store.OpenSession());
        }
    }
}
