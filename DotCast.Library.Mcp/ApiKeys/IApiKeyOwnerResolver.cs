using DotCast.Infrastructure.AppUser;
namespace DotCast.Library.Mcp.ApiKeys;
public interface IApiKeyOwnerResolver
{
    Task<UserInfo?> FindAsync(string ownerId, CancellationToken cancellationToken);
}
