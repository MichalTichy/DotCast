using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class MartenWriteEventRepository : IWriteEventRepository
    {
        private readonly IAsyncSessionFactory sessionFactory;

        public MartenWriteEventRepository(IAsyncSessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public async Task AddEventAsync<TEvent>(string streamId, TEvent @event, CancellationToken cancellationToken = default)
        {
            if (@event is null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            await using var session = await sessionFactory.OpenSessionAsync();
            session.Events.Append(streamId, @event);
            await session.SaveChangesAsync(cancellationToken);
        }
    }
}