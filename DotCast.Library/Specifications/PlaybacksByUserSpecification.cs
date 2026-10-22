using DotCast.Infrastructure.Persistence.Specifications;
using DotCast.SharedKernel.Models;
using Marten;

namespace DotCast.Library.Specifications
{
    internal record PlaybacksByUserSpecification(string UserId) : IListSpecification<AudioBookPlayback>
    {
        public async Task<IReadOnlyList<AudioBookPlayback>> ApplyAsync(IQueryable<AudioBookPlayback> queryable, CancellationToken cancellationToken = default)
        {
            return await queryable.Where(x => x.UserId == UserId).ToListAsync(cancellationToken);
        }
    }
}
