using System.Threading;
using System.Threading.Tasks;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public interface IReadEventRepository<T>
    {
        Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default, string? tenantId = null);
    }
}
