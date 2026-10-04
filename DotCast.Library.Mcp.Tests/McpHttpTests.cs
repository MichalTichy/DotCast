using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DotCast.Infrastructure.PersonalApiTokens.Persistence;
using DotCast.Infrastructure.CurrentTenancyProvider;
using DotCast.Library.Mcp.Persistence;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;
namespace DotCast.Library.Mcp.Tests;
public sealed class McpHttpTests(McpHostFixture fixture) : IClassFixture<McpHostFixture>
{
    private async Task<McpClient> ConnectAsync(string credential) => await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions {
        Endpoint = new Uri(fixture.Address, "/mcp"), AdditionalHeaders = new Dictionary<string, string> { ["Authorization"] = "Bearer " + credential }
    }));
    [Fact]
    public async Task RealClientReadsAccessibleBooksAndCannotWriteWithReadToken()
    {
        var token = await fixture.IssueAsync();
        await using var client = await ConnectAsync(token.Credential);
        var tools = await client.ListToolsAsync();
        Assert.Equal(4, tools.Count);
        var suggestions = await client.CallToolAsync("get_metadata_suggestions", new Dictionary<string, object?> { ["title"] = "Title", ["count"] = 2 });
        Assert.False(suggestions.IsError == true, Text(suggestions));
        Assert.Equal(2, suggestions.StructuredContent!.Value.GetProperty("suggestions").GetArrayLength());
        var search = await client.CallToolAsync("search_audiobooks", new Dictionary<string, object?> { ["search"] = "title" });
        Assert.False(search.IsError == true, Text(search));
        Assert.Equal(2, search.StructuredContent!.Value.GetProperty("total").GetInt32());
        var paged = await client.CallToolAsync("search_audiobooks", new Dictionary<string, object?> { ["search"] = "title", ["limit"] = 1, ["offset"] = 1 });
        Assert.Equal(1, paged.StructuredContent!.Value.GetProperty("items").GetArrayLength());
        var inaccessible = await client.CallToolAsync("search_audiobooks", new Dictionary<string, object?> { ["libraryId"] = "foreign" });
        Assert.Equal(0, inaccessible.StructuredContent!.Value.GetProperty("total").GetInt32());
        var injection = await client.CallToolAsync("search_audiobooks", new Dictionary<string, object?> { ["search"] = "' OR 1=1 --" });
        Assert.Equal(0, injection.StructuredContent!.Value.GetProperty("total").GetInt32());
        var detail = await client.CallToolAsync("get_audiobook", new Dictionary<string, object?> { ["id"] = "own-book" });
        Assert.False(detail.IsError == true, Text(detail));
        var foreign = await client.CallToolAsync("get_audiobook", new Dictionary<string, object?> { ["id"] = "foreign-book" });
        AssertError(foreign, "not_found");
        var write = await client.CallToolAsync("update_audiobook_metadata", new Dictionary<string, object?> {
            ["id"] = "own-book", ["changes"] = new { title = "Should not save" } });
        AssertError(write, "access_denied");
    }
    [Fact]
    public async Task WriteTokenCanUpdateOwnBookButNotSharedBook()
    {
        var token = await fixture.IssueAsync(true);
        await using var client = await ConnectAsync(token.Credential);
        var write = await client.CallToolAsync("update_audiobook_metadata", new Dictionary<string, object?> {
            ["id"] = "own-book", ["changes"] = new { description = "Maintained by MCP" } });
        Assert.False(write.IsError == true, Text(write));
        Assert.Equal("Maintained by MCP", write.StructuredContent!.Value.GetProperty("description").GetString());
        var shared = await client.CallToolAsync("update_audiobook_metadata", new Dictionary<string, object?> {
            ["id"] = "shared-book", ["changes"] = new { title = "Should not save" } });
        AssertError(shared, "not_found");
        var invalid = await client.CallToolAsync("update_audiobook_metadata", new Dictionary<string, object?> {
            ["id"] = "own-book", ["changes"] = new { libraryId = "foreign" } });
        AssertError(invalid, "invalid_input");
    }
    [Fact]
    public async Task RevocationAndSharingRemovalApplyToNextRequest()
    {
        var token = await fixture.IssueAsync();
        await using var client = await ConnectAsync(token.Credential);
        fixture.Owners.Users["owner"].SharedLibraries.Clear();
        try
        {
            AssertError(await client.CallToolAsync("get_audiobook", new Dictionary<string, object?> { ["id"] = "shared-book" }), "not_found");
        }
        finally { fixture.Owners.Users["owner"].SharedLibraries = ["shared"]; }
        using var scope = fixture.App.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IPersonalApiTokenStore>().RevokeAsync(token.Id, "owner", DateTimeOffset.UtcNow, CancellationToken.None);
        await Assert.ThrowsAnyAsync<Exception>(async () => await client.ListToolsAsync());
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Credential);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(new Uri(fixture.Address, "/mcp"), new { jsonrpc = "2.0", id = 1, method = "tools/list" })).StatusCode);
    }
    [Fact]
    public async Task AuthenticationHostBodyAndRateLimitsAreEnforced()
    {
        using var http = new HttpClient();
        var endpoint = new Uri(fixture.Address, "/mcp");
        http.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/event-stream");
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(endpoint, new { })).StatusCode);
        var expired = await fixture.IssueAsync(expiry: DateTimeOffset.UtcNow.AddDays(-1));
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expired.Credential);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(endpoint, new { })).StatusCode);
        var token = await fixture.IssueAsync();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Credential);
        http.DefaultRequestHeaders.Host = "attacker.example";
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync(endpoint, new { })).StatusCode);
        http.DefaultRequestHeaders.Host = null;
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, (await http.PostAsync(endpoint, new StringContent(new string('x', 65537), Encoding.UTF8, "application/json"))).StatusCode);
        var chunked = new UnknownLengthContent(Encoding.UTF8.GetBytes("{\"padding\":\"" + new string('x', 65537) + "\"}"));
        chunked.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, (await http.PostAsync(endpoint, chunked)).StatusCode);
        HttpStatusCode last = default;
        for (var i = 0; i < 61; i++) last = (await http.PostAsJsonAsync(endpoint, new { jsonrpc = "2.0", id = i, method = "tools/list" })).StatusCode;
        Assert.Equal(HttpStatusCode.TooManyRequests, last);
    }
    [Fact]
    public async Task CookieManagementRequiresCsrfAndNeverListsCredential()
    {
        using var http = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false });
        await http.GetAsync(new Uri(fixture.Address, "/fixture/login/owner"));
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(new Uri(fixture.Address, "/mcp"), new { })).StatusCode);
        var management = new Uri(fixture.Address, "/api/personal-tokens");
        var body = new { name = "Test client", expiresAt = DateTimeOffset.UtcNow.AddDays(90), canWrite = false };
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync(management, body)).StatusCode);
        var list = await http.GetFromJsonAsync<JsonElement>(management);
        http.DefaultRequestHeaders.Add("RequestVerificationToken", list.GetProperty("requestToken").GetString());
        var createdResponse = await http.PostAsJsonAsync(management, body);
        Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
        var created = await createdResponse.Content.ReadFromJsonAsync<JsonElement>();
        var credential = created.GetProperty("credential").GetString()!;
        var id = created.GetProperty("info").GetProperty("id").GetString()!;
        var listed = await http.GetStringAsync(management);
        Assert.DoesNotContain(credential, listed);
        Assert.DoesNotContain("secretHash", listed, StringComparison.OrdinalIgnoreCase);
        Assert.All(fixture.Logs.Messages, log => Assert.DoesNotContain(credential, log));
        var invalidBody = new { name = "Invalid expiry", expiresAt = DateTimeOffset.UtcNow.AddDays(366), canWrite = false };
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync(management, invalidBody)).StatusCode);
        using var scope = fixture.App.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IPersonalApiTokenStore>();
        Assert.False(await store.RevokeAsync(id, "foreign-owner", DateTimeOffset.UtcNow, CancellationToken.None));
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync(new Uri(fixture.Address, "/api/personal-tokens/" + id))).StatusCode);
        Assert.NotNull((await store.FindAsync(id, CancellationToken.None))!.RevokedAt);
    }
    [Fact]
    public async Task RemovedAndLockedOwnersCannotAuthenticate()
    {
        var token = await fixture.IssueAsync();
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Credential);
        var endpoint = new Uri(fixture.Address, "/mcp");
        fixture.Owners.LockedOwners.Add("owner");
        try { Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(endpoint, new { })).StatusCode); }
        finally { fixture.Owners.LockedOwners.Remove("owner"); }
        fixture.Owners.Users.TryRemove("owner", out var user);
        try { Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(endpoint, new { })).StatusCode); }
        finally { fixture.Owners.Users["owner"] = user!; }
    }
    [Fact]
    public async Task PersonalTokenCannotManageTokensAndInsecureHttpRequiresDevelopmentOptIn()
    {
        var token = await fixture.IssueAsync(true);
        using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Credential);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync(new Uri(fixture.Address, "/api/personal-tokens"))).StatusCode);
        fixture.App.Configuration["Mcp:AllowLoopbackHttp"] = "false";
        try { Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync(new Uri(fixture.Address, "/mcp"), new { })).StatusCode); }
        finally { fixture.App.Configuration["Mcp:AllowLoopbackHttp"] = "true"; }
    }
    [Fact]
    public async Task ConcurrentPatchesPreserveBothFields()
    {
        var store = fixture.App.Services.GetRequiredService<IDocumentStore>();
        await using (var session = store.LightweightSession(CurrentTenancyProviderNoTenancy.NoTenancyName))
        { session.Store(MetadataPatchTests.Book("concurrent", "own")); await session.SaveChangesAsync(); }
        var writer = new TransactionalAudioBookMetadataWriter(store, new CurrentTenancyProviderNoTenancy());
        await Task.WhenAll(
            writer.UpdateAsync("concurrent", "own", MetadataPatchTests.Patch("""{"title":"Updated title"}"""), CancellationToken.None),
            writer.UpdateAsync("concurrent", "own", MetadataPatchTests.Patch("""{"description":"Updated description"}"""), CancellationToken.None));
        await using var read = store.QuerySession(CurrentTenancyProviderNoTenancy.NoTenancyName);
        var book = await read.LoadAsync<DotCast.SharedKernel.Models.AudioBook>("concurrent");
        Assert.Equal("Updated title", book!.AudioBookInfo.Name);
        Assert.Equal("Updated description", book.AudioBookInfo.Description);
        await using var cleanup = store.LightweightSession(CurrentTenancyProviderNoTenancy.NoTenancyName);
        cleanup.Delete(book);
        await cleanup.SaveChangesAsync();
    }
    private static string Text(CallToolResult result) => string.Join(" ", result.Content.OfType<TextContentBlock>().Select(t => t.Text));
    private static void AssertError(CallToolResult result, string code)
    {
        Assert.True(result.IsError, Text(result));
        Assert.Equal(code, result.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString());
    }
}
