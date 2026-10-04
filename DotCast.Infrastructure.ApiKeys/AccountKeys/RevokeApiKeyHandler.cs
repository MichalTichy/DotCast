using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;
namespace DotCast.Infrastructure.ApiKeys;
public sealed class RevokeApiKeyHandler(IRepository<AccountApiKey> keys, ICurrentUserProvider<UserInfo> users, ILogger<RevokeApiKeyHandler> logger)
{
    public async Task Handle(RevokeApiKey request, CancellationToken cancellationToken)
    {
        var owner = await users.GetCurrentUserRequiredAsync();
        await keys.DeleteByIdAsync(owner.Id, cancellationToken);
        logger.LogInformation("API key revoked for account {OwnerId}", owner.Id);
    }
}
