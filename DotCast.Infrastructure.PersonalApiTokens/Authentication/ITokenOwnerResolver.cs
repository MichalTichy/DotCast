using DotCast.Infrastructure.AppUser;
namespace DotCast.Infrastructure.PersonalApiTokens.Authentication;
public interface ITokenOwnerResolver
{
    Task<UserInfo?> FindAsync(string ownerId, CancellationToken cancellationToken);
}
