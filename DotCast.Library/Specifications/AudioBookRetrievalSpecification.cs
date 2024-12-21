using DotCast.SharedKernel.Models;
using Marten;
using Shared.Infrastructure.Persistence.Specifications;

namespace DotCast.Library.Specifications
{
    internal record AudioBookRetrievalSpecification(string? Filter) : IListSpecification<AudioBook>
    {
        public Task<IReadOnlyList<AudioBook>> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            var finalQuery = queryable;
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                finalQuery = finalQuery.Where(x =>
                    x.AudioBookInfo.Name.Contains(Filter, StringComparison.InvariantCultureIgnoreCase) ||
                    x.AudioBookInfo.AuthorName.Contains(Filter, StringComparison.InvariantCultureIgnoreCase) ||
                    (x.AudioBookInfo.Description != null && x.AudioBookInfo.Description.Contains(Filter, StringComparison.InvariantCultureIgnoreCase)) ||
                    x.AudioBookInfo.Categories.Any(category => category.Name.Contains(Filter, StringComparison.InvariantCultureIgnoreCase)));
            }

            return finalQuery.ToListAsync(cancellationToken);
        }
    }
}