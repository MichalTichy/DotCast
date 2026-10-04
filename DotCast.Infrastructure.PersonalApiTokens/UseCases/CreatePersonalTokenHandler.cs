using DotCast.Infrastructure.PersonalApiTokens.Authentication;
using DotCast.Infrastructure.PersonalApiTokens.Models;
using DotCast.Infrastructure.PersonalApiTokens.Persistence;
using DotCast.Infrastructure.PersonalApiTokens.Texts;
using Microsoft.Extensions.Logging;
namespace DotCast.Infrastructure.PersonalApiTokens.UseCases;
public sealed class CreatePersonalTokenHandler(IPersonalApiTokenStore tokens, TokenManagementAccess access, TimeProvider time, ILogger<CreatePersonalTokenHandler> logger)
{
    public async Task<CreatedPersonalToken> Handle(CreatePersonalToken request, CancellationToken cancellationToken)
    {
        var owner = await access.RequireOwnerAsync();
        var now = time.GetUtcNow();
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100 ||
            request.ExpiresAt <= now || request.ExpiresAt > now.AddDays(365))
            throw new ArgumentException(TokenTexts.Get("InvalidToken"));
        var generated = PersonalTokenCredential.Generate();
        var token = new PersonalApiToken { Id = generated.Id, OwnerId = owner, Name = request.Name.Trim(),
            SecretHash = generated.Hash, CanWrite = request.CanWrite, CreatedAt = now, ExpiresAt = request.ExpiresAt };
        await tokens.AddAsync(token, cancellationToken);
        logger.LogInformation("Personal API token {TokenId} created for owner {OwnerId}", token.Id, owner);
        return new(generated.Credential, new(token.Id, token.Name, token.CanWrite, token.CreatedAt, token.ExpiresAt, null));
    }
}
