using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
namespace DotCast.Library.Mcp.Tests;
public sealed class UnauthenticatedRateLimitTests : IClassFixture<McpHostFixture>
{
    private readonly McpHostFixture fixture;
    public UnauthenticatedRateLimitTests(McpHostFixture fixture) => this.fixture = fixture;
    [Fact]
    public async Task InvalidCredentialsAreLimitedBySourceIp()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid");
        var endpoint = new Uri(fixture.Address, "/mcp");
        for (var attempt = 0; attempt < 20; attempt++)
        {
            using var response = await http.PostAsJsonAsync(endpoint, new { });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);
        }
        using var limited = await http.PostAsJsonAsync(endpoint, new { });
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }
}
