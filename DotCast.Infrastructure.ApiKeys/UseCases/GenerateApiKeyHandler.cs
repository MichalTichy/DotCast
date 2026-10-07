using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;
namespace DotCast.Infrastructure.ApiKeys;
public sealed class GenerateApiKeyHandler(IRepository<AccountApiKey> keys, ICurrentUserProvider<UserInfo> users, ILogger<GenerateApiKeyHandler> logger)
{
    public async Task<string> Handle(GenerateApiKey request, CancellationToken cancellationToken)
    {
        var owner = await users.GetCurrentUserRequiredAsync();
        var key = ApiKeyCredential.Generate();
        ApiKeyCredential.TryHash(key, out var hash);
        // The account ID is the document ID: one atomic upsert replaces the old key.
        await keys.UpsertAsync(new AccountApiKey { Id = owner.Id, Hash = hash, CreatedAt = DateTimeOffset.UtcNow }, cancellationToken);
        logger.LogInformation("API key generated for account {OwnerId}", owner.Id);
        return key;
    }
}
