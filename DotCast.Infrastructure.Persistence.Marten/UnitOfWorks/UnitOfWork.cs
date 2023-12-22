using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Npgsql;

namespace DotCast.Infrastructure.Persistence.Marten.UnitOfWorks
{
    public class UnitOfWork : IAsyncDisposable
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly string creationStackTrace;
#pragma warning restore IDE0052 // Remove unread private members
        public Guid OwnerId { get; }

        internal NpgsqlConnection? Connection { get; private set; }
        internal NpgsqlTransaction? Transaction { get; private set; }

        internal UnitOfWork(Guid ownerId)
        {
            OwnerId = ownerId;
#if DEBUG
            creationStackTrace = new StackTrace().ToString();
#else
            creationStackTrace = "Only in debug";
#endif
        }

        public async ValueTask DisposeAsync()
        {
            if (Transaction != null)
            {
                await Transaction.DisposeAsync();
            }

            if (Connection != null)
            {
                await Connection.DisposeAsync();
            }
        }

        internal async Task CommitAsync()
        {
            if (Transaction != null)
            {
                await Transaction.CommitAsync();
            }
        }

        internal async Task EnsureInitializedAsync(Func<Task<NpgsqlConnection>> connectionFactory, IsolationLevel preferredIsolationLevel)
        {
            if (Connection == null)
            {
                Connection = await connectionFactory();
                if (Connection.State != ConnectionState.Open)
                {
                    throw new ArgumentException("Created connection is not open.", nameof(connectionFactory));
                }

                Transaction = await Connection.BeginTransactionAsync(preferredIsolationLevel);
            }
        }
    }
}