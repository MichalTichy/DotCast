using System.Threading;
using System.Threading.Tasks;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public interface IWriteEventRepository
    {
        Task AddEventAsync<TEvent>(string streamId, TEvent @event, CancellationToken cancellationToken = default);
    }
}