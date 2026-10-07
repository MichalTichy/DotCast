using DotCast.Infrastructure.Persistence.Specifications;
using DotCast.SharedKernel.Models;
using Marten;

namespace DotCast.Library.Specifications
{
    internal record UserAudioBooksByUserSpecification(string UserId) : IListSpecification<UserAudioBook>
    {
        public async Task<IReadOnlyList<UserAudioBook>> ApplyAsync(IQueryable<UserAudioBook> queryable, CancellationToken cancellationToken = default)
        {
            return await queryable.Where(x => x.UserId == UserId).ToListAsync(cancellationToken);
        }
    }
}
