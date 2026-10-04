using DotCast.Infrastructure.PersonalApiTokens.Models;
namespace DotCast.Infrastructure.PersonalApiTokens.Persistence;
public interface IPersonalApiTokenStore
{
    Task<PersonalApiToken?> FindAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PersonalTokenInfo>> ListAsync(string ownerId, CancellationToken cancellationToken);
    Task AddAsync(PersonalApiToken token, CancellationToken cancellationToken);
    Task<bool> RevokeAsync(string id, string ownerId, DateTimeOffset now, CancellationToken cancellationToken);
}
