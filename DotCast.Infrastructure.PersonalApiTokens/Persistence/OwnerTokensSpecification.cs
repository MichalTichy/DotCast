using DotCast.Infrastructure.Persistence.Specifications;
using DotCast.Infrastructure.PersonalApiTokens.Models;
using Marten;
namespace DotCast.Infrastructure.PersonalApiTokens.Persistence;
public sealed record OwnerTokensSpecification(string OwnerId) : IListSpecification<PersonalApiToken, PersonalTokenInfo>
{
    public async Task<IReadOnlyList<PersonalTokenInfo>> ApplyAsync(IQueryable<PersonalApiToken> queryable, CancellationToken cancellationToken = default) =>
        await queryable.Where(t => t.OwnerId == OwnerId).OrderByDescending(t => t.CreatedAt)
            .Select(t => new PersonalTokenInfo(t.Id, t.Name, t.CanWrite, t.CreatedAt, t.ExpiresAt, t.RevokedAt))
            .ToListAsync(cancellationToken);
}
