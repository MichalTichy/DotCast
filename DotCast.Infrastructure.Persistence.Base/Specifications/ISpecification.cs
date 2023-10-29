namespace DotCast.Infrastructure.Persistence.Base.Specifications
{
    public interface ISpecification<T> : ISpecification<T, T>
    {
    }

    public interface ISpecification<TItem, TOut>
    {
        Task<TOut?> ApplyAsync(IQueryable<TItem> queryable, CancellationToken cancellationToken = default);
    }
}
