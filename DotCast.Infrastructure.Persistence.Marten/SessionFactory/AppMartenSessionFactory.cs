using System.Data;
using System.Threading.Tasks;
using DotCast.Infrastructure.Persistence.Marten.UnitOfWorks;
using Marten;
using Marten.Services;
using Npgsql;

namespace DotCast.Infrastructure.Persistence.Marten.SessionFactory
{
    public class AppMartenSessionFactory : IAsyncSessionFactory
    {
        private readonly IConnectionFactory connectionFactory;
        protected readonly IDocumentStore Store;

        public AppMartenSessionFactory(IDocumentStore store, IConnectionFactory connectionFactory)
        {
            Store = store;
            this.connectionFactory = connectionFactory;
        }

        public virtual async Task<IQuerySession> QuerySessionAsync(UnitOfWork unitOfWork)
        {
            var sessionOptions = await CreateSessionOptionsAsync(unitOfWork);

            return Store.QuerySession(sessionOptions);
        }

        public virtual async Task<IDocumentSession> OpenSessionAsync(UnitOfWork unitOfWork)
        {
            var sessionOptions = await CreateSessionOptionsAsync(unitOfWork);

            return await Store.OpenSerializableSessionAsync(sessionOptions);
        }

        protected virtual async Task<SessionOptions> CreateSessionOptionsAsync(UnitOfWork unitOfWork)
        {
            await unitOfWork.EnsureInitializedAsync(CreateNewConnectionAsync, GetIsolationLevel());
            var options = SessionOptions.ForTransaction(unitOfWork.Transaction!);

            options.IsolationLevel = GetIsolationLevel();

            return options;
        }

        private async Task<NpgsqlConnection> CreateNewConnectionAsync()
        {
            var connection = connectionFactory.Create();
            await connection.OpenAsync();
            return connection;
        }

        private IsolationLevel GetIsolationLevel()
        {
            return IsolationLevel.ReadCommitted;
        }
    }
}