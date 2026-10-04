using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentUserProvider;
using Marten;
using Microsoft.Extensions.Logging;
namespace DotCast.Library.Mcp.ApiKeys;
public sealed class GenerateApiKeyHandler(IDocumentStore documents, ICurrentUserProvider<UserInfo> users, ILogger<GenerateApiKeyHandler> logger)
{
    public async Task<string> Handle(GenerateApiKey request, CancellationToken cancellationToken)
    {
        var owner = await users.GetCurrentUserRequiredAsync();
        var key = ApiKeyCredential.Generate();
        ApiKeyCredential.TryHash(key, out var hash);
        await using var session = documents.LightweightSession();
        // The account ID is the document ID: one atomic upsert replaces the old key.
        session.Store(new AccountApiKey { Id = owner.Id, Hash = hash, CreatedAt = DateTimeOffset.UtcNow });
        await session.SaveChangesAsync(cancellationToken);
        logger.LogInformation("API key generated for account {OwnerId}", owner.Id);
        return key;
    }
}
