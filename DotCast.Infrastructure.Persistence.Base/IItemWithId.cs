namespace DotCast.Infrastructure.Persistence.Base
{
    public interface IItemWithId<TId>
    {
        public TId Id { get; }
    }
}