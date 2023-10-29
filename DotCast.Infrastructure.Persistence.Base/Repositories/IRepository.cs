namespace DotCast.Infrastructure.Persistence.Base.Repositories
{
    public interface IRepository<T> : IReadOnlyRepository<T>
    {
        Task AddAsync(ICollection<T> entities, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<T> StoreAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task ForceUpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    }
}
