namespace DotCast.Infrastructure.Persistence.Base.Repositories
{
    public interface IRepository<T, TId> : IReadOnlyRepository<T, TId> where T : IItemWithId<TId>
    {
        Task AddAsync(ICollection<T> entities, CancellationToken cancellationToken = default, string? tenantId = null);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default, string? tenantId = null);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default, string? tenantId = null);
        Task ForceUpdateAsync(T entity, CancellationToken cancellationToken = default, string? tenantId = null);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default, string? tenantId = null);
    }
}