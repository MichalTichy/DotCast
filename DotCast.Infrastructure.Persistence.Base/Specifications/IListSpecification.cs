namespace DotCast.Infrastructure.Persistence.Base.Specifications
{
    public interface IListSpecification<T> : IListSpecification<T, T>
    {
    }

    public interface IListSpecification<TItem, TOut>
    {
        Task<IReadOnlyList<TOut>> ApplyAsync(IQueryable<TItem> queryable, CancellationToken cancellationToken = default);
    }
}
