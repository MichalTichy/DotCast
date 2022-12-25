using DotCast.Infrastructure.Persistence.Base.Specifications;

namespace DotCast.Infrastructure.Persistence.Base.Repositories
{
    public interface IReadOnlyRepository<T, TId> where T : IItemWithId<TId>
    {
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default, string? tenantId = null);
        Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default, string? tenantId = null);
        Task<T?> GetBySpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default, string? tenantId = null);
        Task<TResult?> GetBySpecAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default, string? tenantId = null);
        Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default, string? tenantId = null);
        Task<IReadOnlyList<T>> ListAsync(IListSpecification<T> specification, CancellationToken cancellationToken = default, string? tenantId = null);
        Task<IReadOnlyList<TResult>> ListAsync<TResult>(IListSpecification<T, TResult> specification, CancellationToken cancellationToken = default, string? tenantId = null);
        Task<int> CountAsync(CancellationToken cancellationToken = default, string? tenantId = null);
    }
}