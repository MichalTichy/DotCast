using DotCast.SharedKernel.Models;
using Marten;
using Shared.Infrastructure.Persistence.Specifications;

namespace DotCast.Library.Specifications
{
    internal record AudioBookExistenceCheckSpecification(string Id) : ISpecification<AudioBook, bool>
    {
        public async Task<bool> ApplyAsync(IQueryable<AudioBook> queryable, CancellationToken cancellationToken = default)
        {
            return await queryable.AnyAsync(t => t.Id == Id, cancellationToken);
        }
    }
}