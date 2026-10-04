using DotCast.Infrastructure.PersonalApiTokens.Authentication;
using DotCast.Infrastructure.PersonalApiTokens.Models;
using Xunit;
namespace DotCast.Library.Mcp.Tests;
public sealed class TokenCredentialTests
{
    [Fact]
    public void GeneratedCredentialVerifiesOnlyTheMatchingSecret()
    {
        var generated = PersonalTokenCredential.Generate();
        Assert.True(PersonalTokenCredential.TryParse(generated.Credential, out var id, out var hash));
        Assert.Equal(generated.Id, id);
        Assert.Equal(generated.Hash, hash);
        var token = Token(generated.Id, generated.Hash);
        Assert.True(PersonalTokenCredential.IsValid(token, hash, DateTimeOffset.UtcNow));
        Assert.False(PersonalTokenCredential.IsValid(token, PersonalTokenCredential.Generate().Hash, DateTimeOffset.UtcNow));
        Assert.DoesNotContain(generated.Credential, Convert.ToHexString(token.SecretHash));
    }
    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData("Bearer secret")] [InlineData("dcpat_v1.id.secret")]
    public void MalformedCredentialsFail(string? credential) => Assert.False(PersonalTokenCredential.TryParse(credential, out _, out _));
    [Fact]
    public void RevokedAndExpiredTokensFail()
    {
        var generated = PersonalTokenCredential.Generate();
        var token = Token(generated.Id, generated.Hash);
        Assert.False(PersonalTokenCredential.IsValid(token, generated.Hash, token.ExpiresAt));
        token.RevokedAt = DateTimeOffset.UtcNow;
        Assert.False(PersonalTokenCredential.IsValid(token, generated.Hash, DateTimeOffset.UtcNow));
    }
    private static PersonalApiToken Token(string id, byte[] hash) => new() {
        Id = id, OwnerId = "owner", Name = "test", SecretHash = hash, CanWrite = false,
        CreatedAt = DateTimeOffset.UtcNow, ExpiresAt = DateTimeOffset.UtcNow.AddDays(90) };
}
