using DotCast.Infrastructure.Persistence.Base.Specifications;
using DotCast.SharedKernel.Models;
using Marten;

namespace DotCast.Library.Specifications
{
    internal record AudioBookRetrievalSpecification(string? Filter) : IListSpecification<AudioBook>
    {
        public Task<IReadOnlyList<AudioBook>> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            var finalQuery = queryable;
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                finalQuery.PlainTextSearch(Filter);
            }

            return finalQuery.ToListAsync(cancellationToken);
        }
    }
}