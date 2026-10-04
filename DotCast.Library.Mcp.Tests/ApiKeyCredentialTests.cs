using DotCast.Library.Mcp.ApiKeys;
using Xunit;
namespace DotCast.Library.Mcp.Tests;
public sealed class ApiKeyCredentialTests
{
    [Fact]
    public void KeysAreRandomAndOnlyTheirDigestIsStored()
    {
        var key = ApiKeyCredential.Generate();
        Assert.True(ApiKeyCredential.TryHash(key, out var hash));
        Assert.Equal(64, hash.Length);
        Assert.DoesNotContain(key, hash);
        Assert.NotEqual(key, ApiKeyCredential.Generate());
        Assert.True(ApiKeyCredential.TryHash(key, out var sameHash));
        Assert.Equal(hash, sameHash);
    }
    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData("Bearer secret")] [InlineData("dcak_invalid")]
    public void MalformedKeysFail(string? key) => Assert.False(ApiKeyCredential.TryHash(key, out _));
}
