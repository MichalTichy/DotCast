using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Infrastructure.PersonalApiTokens.Models;
namespace DotCast.Infrastructure.PersonalApiTokens.Persistence;
public sealed class PersonalApiTokenStore(IRepository<PersonalApiToken> repository) : IPersonalApiTokenStore
{
    public Task<PersonalApiToken?> FindAsync(string id, CancellationToken cancellationToken) => repository.GetByIdAsync(id, cancellationToken);
    public Task<IReadOnlyList<PersonalTokenInfo>> ListAsync(string ownerId, CancellationToken cancellationToken) => repository.ListAsync(new OwnerTokensSpecification(ownerId), cancellationToken);
    public async Task AddAsync(PersonalApiToken token, CancellationToken cancellationToken) => await repository.AddAsync(token, cancellationToken);
    public async Task<bool> RevokeAsync(string id, string ownerId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var token = await repository.GetByIdAsync(id, cancellationToken);
        if (token is null || token.OwnerId != ownerId) return false;
        if (token.RevokedAt is null)
        {
            token.RevokedAt = now;
            await repository.UpdateAsync(token, cancellationToken);
        }
        return true;
    }
}
