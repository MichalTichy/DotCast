using DotCast.Infrastructure.AppUser;
namespace DotCast.Infrastructure.ApiKeys;
public interface IApiKeyOwnerResolver
{
    Task<UserInfo?> FindAsync(string ownerId, CancellationToken cancellationToken);
}
