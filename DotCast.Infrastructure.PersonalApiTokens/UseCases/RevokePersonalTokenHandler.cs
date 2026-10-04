using DotCast.Infrastructure.PersonalApiTokens.Persistence;
using Microsoft.Extensions.Logging;
namespace DotCast.Infrastructure.PersonalApiTokens.UseCases;
public sealed class RevokePersonalTokenHandler(IPersonalApiTokenStore tokens, TokenManagementAccess access, TimeProvider time, ILogger<RevokePersonalTokenHandler> logger)
{
    public async Task Handle(RevokePersonalToken request, CancellationToken cancellationToken)
    {
        var owner = await access.RequireOwnerAsync();
        if (!await tokens.RevokeAsync(request.Id, owner, time.GetUtcNow(), cancellationToken)) throw new KeyNotFoundException();
        logger.LogInformation("Personal API token {TokenId} revoked for owner {OwnerId}", request.Id, owner);
    }
}
