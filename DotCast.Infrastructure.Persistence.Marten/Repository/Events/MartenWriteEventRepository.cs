using DotCast.Infrastructure.Persistence.Marten.SessionFactory;
using DotCast.Infrastructure.Persistence.Marten.UnitOfWorks;

namespace DotCast.Infrastructure.Persistence.Marten.Repository.Events;

public class MartenWriteEventRepository(IAsyncSessionFactory sessionFactory) : IWriteEventRepository
{
    public async Task AddEventAsync<TEvent>(string streamId, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        await using var uow = new UnitOfWorkProvider();
        await using var session = await sessionFactory.OpenSessionAsync(uow.Get());

        session.Events.Append(streamId, @event);

        await session.SaveChangesAsync(cancellationToken);
        await uow.CommitAsync();
    }
}
