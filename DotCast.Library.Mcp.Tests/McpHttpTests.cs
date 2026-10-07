using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DotCast.Infrastructure.CurrentTenancyProvider;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Infrastructure.ApiKeys;
using DotCast.Library.Mcp.UseCases;
using DotCast.Infrastructure.Blazor.ClaimsManagement;
using DotCast.SharedKernel.Models;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;
namespace DotCast.Library.Mcp.Tests;
public sealed class McpHttpTests(McpHostFixture fixture) : IClassFixture<McpHostFixture>
{
    private async Task<McpClient> ConnectAsync(string key) => await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions {
        Endpoint = new Uri(fixture.Address, "/mcp"), AdditionalHeaders = new Dictionary<string, string> { ["Authorization"] = "Bearer " + key }
    }));
    [Fact]
    public async Task KeyCanReadAccessibleBooksAndUpdateOnlyItsOwnLibrary()
    {
        await using var client = await ConnectAsync(await fixture.IssueAsync());
        Assert.Equal(4, (await client.ListToolsAsync()).Count);
        var suggestions = await client.CallToolAsync("get_metadata_suggestions", new Dictionary<string, object?> { ["title"] = "Title", ["count"] = 2 });
        Assert.False(suggestions.IsError == true, Text(suggestions));
        Assert.Equal(2, suggestions.StructuredContent!.Value.GetProperty("suggestions").GetArrayLength());
        var search = await client.CallToolAsync("search_audiobooks", new Dictionary<string, object?> { ["search"] = "title" });
        Assert.Equal(2, search.StructuredContent!.Value.GetProperty("total").GetInt32());
        var paged = await client.CallToolAsync("search_audiobooks", new Dictionary<string, object?> { ["search"] = "title", ["limit"] = 1, ["offset"] = 1 });
        Assert.Equal(1, paged.StructuredContent!.Value.GetProperty("items").GetArrayLength());
        var inaccessible = await client.CallToolAsync("search_audiobooks", new Dictionary<string, object?> { ["libraryId"] = "foreign" });
        Assert.Equal(0, inaccessible.StructuredContent!.Value.GetProperty("total").GetInt32());
        var injection = await client.CallToolAsync("search_audiobooks", new Dictionary<string, object?> { ["search"] = "' OR 1=1 --" });
        Assert.Equal(0, injection.StructuredContent!.Value.GetProperty("total").GetInt32());
        Assert.False((await client.CallToolAsync("get_audiobook", new Dictionary<string, object?> { ["id"] = "own-book" })).IsError == true);
        AssertError(await client.CallToolAsync("get_audiobook", new Dictionary<string, object?> { ["id"] = "foreign-book" }), "not_found");
        var write = await client.CallToolAsync("update_audiobook_metadata", new Dictionary<string, object?> {
            ["id"] = "own-book", ["changes"] = new { description = "Maintained by MCP" } });
        Assert.False(write.IsError == true, Text(write));
        Assert.Equal("Maintained by MCP", write.StructuredContent!.Value.GetProperty("description").GetString());
        AssertError(await client.CallToolAsync("update_audiobook_metadata", new Dictionary<string, object?> {
            ["id"] = "shared-book", ["changes"] = new { title = "Should not save" } }), "not_found");
        AssertError(await client.CallToolAsync("update_audiobook_metadata", new Dictionary<string, object?> {
            ["id"] = "foreign-book", ["changes"] = new { title = "Should not save" } }), "not_found");
        AssertError(await client.CallToolAsync("update_audiobook_metadata", new Dictionary<string, object?> {
            ["id"] = "missing-book", ["changes"] = new { title = "Should not save" } }), "not_found");
        AssertError(await client.CallToolAsync("update_audiobook_metadata", new Dictionary<string, object?> {
            ["id"] = "own-book", ["changes"] = new { libraryId = "foreign" } }), "invalid_input");
    }
    [Fact]
    public async Task ConcurrentRegenerationLeavesOnlyOneUsableKey()
    {
        var keys = await Task.WhenAll(fixture.IssueAsync(), fixture.IssueAsync());
        var results = new List<HttpStatusCode>();
        foreach (var key in keys)
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/event-stream");
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            using var response = await http.PostAsJsonAsync(new Uri(fixture.Address, "/mcp"),
                new { jsonrpc = "2.0", id = 1, method = "tools/list" });
            results.Add(response.StatusCode);
        }
        Assert.Single(results, status => status == HttpStatusCode.OK);
        Assert.Single(results, status => status == HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task RegenerationReplacesTheOnlyKeyAndRevocationAppliesToTheNextRequest()
    {
        var oldKey = await fixture.IssueAsync();
        await using var oldClient = await ConnectAsync(oldKey);
        var key = await fixture.IssueAsync();
        await Assert.ThrowsAnyAsync<Exception>(async () => await oldClient.ListToolsAsync());
        await using var client = await ConnectAsync(key);
        using var scope = fixture.App.Services.CreateScope();
        var keys = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<AccountApiKey>>();
        Assert.Equal(1, await keys.CountAsync());
        var document = await keys.GetByIdAsync("owner");
        ApiKeyCredential.TryHash(key, out var hash);
        Assert.Equal(hash, document!.Hash);
        Assert.DoesNotContain(key, JsonSerializer.Serialize(document));
        var info = await keys.GetBySpecAsync(new AccountApiKeyInfoSpecification("owner"));
        Assert.DoesNotContain(hash, JsonSerializer.Serialize(info));
        Assert.All(fixture.Logs.Messages, log => { Assert.DoesNotContain(key, log); Assert.DoesNotContain(oldKey, log); });
        // Other accounts can only revoke their own key.
        await fixture.RevokeAsync("other");
        Assert.Equal(4, (await client.ListToolsAsync()).Count);
        await fixture.RevokeAsync();
        await Assert.ThrowsAnyAsync<Exception>(async () => await client.ListToolsAsync());
        Assert.Null(await keys.GetByIdAsync("owner"));
    }
    [Fact]
    public async Task SharingRemovalAppliesToTheNextRequest()
    {
        await using var client = await ConnectAsync(await fixture.IssueAsync());
        fixture.Owners.Users["owner"].SharedLibraries.Clear();
        try { AssertError(await client.CallToolAsync("get_audiobook", new Dictionary<string, object?> { ["id"] = "shared-book" }), "not_found"); }
        finally { fixture.Owners.Users["owner"].SharedLibraries = ["shared"]; }
    }
    [Fact]
    public async Task AuthenticationHostBodyAndRateLimitsAreEnforced()
    {
        using var http = new HttpClient();
        var endpoint = new Uri(fixture.Address, "/mcp");
        http.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/event-stream");
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(endpoint, new { })).StatusCode);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await fixture.IssueAsync());
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
    public async Task RemovedAndLockedOwnersCannotAuthenticate()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await fixture.IssueAsync());
        var endpoint = new Uri(fixture.Address, "/mcp");
        fixture.Owners.LockedOwners.Add("owner");
        try { Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(endpoint, new { })).StatusCode); }
        finally { fixture.Owners.LockedOwners.Remove("owner"); }
        fixture.Owners.Users.TryRemove("owner", out var user);
        try { Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(endpoint, new { })).StatusCode); }
        finally { fixture.Owners.Users["owner"] = user!; }
    }
    [Fact]
    public async Task CookieAloneCannotUseMcpAndKeysRequireAnAuthenticatedAccount()
    {
        using var http = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false });
        await http.GetAsync(new Uri(fixture.Address, "/fixture/login/owner"));
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(new Uri(fixture.Address, "/mcp"), new { })).StatusCode);
        await Assert.ThrowsAnyAsync<UnauthorizedAccessException>(() => fixture.RunAsAsync(null,
            messenger => messenger.RequestAsync<GenerateApiKey, string>(new())));
    }
    [Fact]
    public async Task HttpAllowsConfiguredPublicHostAndStillRequiresAKey()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Host = "dotcast.example";
        http.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/event-stream");
        var allowedHosts = fixture.App.Configuration["Mcp:AllowedHosts"];
        fixture.App.Configuration["Mcp:AllowedHosts"] = "dotcast.example";
        try
        {
            var endpoint = new Uri(fixture.Address, "/mcp");
            Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(endpoint, new { })).StatusCode);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await fixture.IssueAsync());
            await using var client = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions { Endpoint = endpoint }, http));
            Assert.Equal(4, (await client.ListToolsAsync()).Count);
        }
        finally { fixture.App.Configuration["Mcp:AllowedHosts"] = allowedHosts; }
    }
    [Fact]
    public async Task ConcurrentPatchesPreserveBothFields()
    {
        var store = fixture.App.Services.GetRequiredService<IDocumentStore>();
        await using (var session = store.LightweightSession(CurrentTenancyProviderNoTenancy.NoTenancyName))
        { session.Store(MetadataPatchTests.Book("concurrent", "own")); await session.SaveChangesAsync(); }
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await Task.WhenAll(
            fixture.RunAsAsync("owner", messenger => messenger.RequestAsync<UpdateAudioBookMetadata, DotCast.Library.Mcp.Models.AudioBookMetadata>(
                new("concurrent", MetadataPatchTests.Patch("""{"title":"Updated title"}""")), timeout.Token)),
            fixture.RunAsAsync("owner", messenger => messenger.RequestAsync<UpdateAudioBookMetadata, DotCast.Library.Mcp.Models.AudioBookMetadata>(
                new("concurrent", MetadataPatchTests.Patch("""{"description":"Updated description"}""")), timeout.Token)));
        await using var read = store.QuerySession(CurrentTenancyProviderNoTenancy.NoTenancyName);
        var book = await read.LoadAsync<DotCast.SharedKernel.Models.AudioBook>("concurrent");
        Assert.Equal("Updated title", book!.AudioBookInfo.Name);
        Assert.Equal("Updated description", book.AudioBookInfo.Description);
        await using var cleanup = store.LightweightSession(CurrentTenancyProviderNoTenancy.NoTenancyName);
        cleanup.Delete(book);
        await cleanup.SaveChangesAsync();
    }
    [Fact]
    public async Task RepositoryRetriesConflictingUpdatesAndStillSupportsDetachedUpdates()
    {
        var store = fixture.App.Services.GetRequiredService<IDocumentStore>();
        await using (var session = store.LightweightSession(CurrentTenancyProviderNoTenancy.NoTenancyName))
        { session.Store(MetadataPatchTests.Book("repository-concurrent", "own")); await session.SaveChangesAsync(); }
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var loaded = 0;
        var bothLoaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task PatchAsync(Action<AudioBook> patch)
        {
            using var scope = fixture.App.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<IUserClaimsProvider>().User = fixture.Owners.Users["owner"].GetClaimsIdentity();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<AudioBook>>();
            await repository.GetAndUpdateAsync("repository-concurrent", async book => {
                // Force both writers to load the same version, so one must retry.
                var attempt = Interlocked.Increment(ref loaded);
                if (attempt == 2) bothLoaded.TrySetResult();
                if (attempt <= 2) await bothLoaded.Task.WaitAsync(timeout.Token);
                patch(book);
            }, timeout.Token);
        }
        await Task.WhenAll(PatchAsync(book => book.AudioBookInfo.Name = "Retried title"),
            PatchAsync(book => book.AudioBookInfo.Description = "Retried description"));
        Assert.True(loaded >= 3);
        using var readScope = fixture.App.Services.CreateScope();
        readScope.ServiceProvider.GetRequiredService<IUserClaimsProvider>().User = fixture.Owners.Users["owner"].GetClaimsIdentity();
        var books = readScope.ServiceProvider.GetRequiredService<IRepository<AudioBook>>();
        var result = (await books.GetByIdAsync("repository-concurrent"))!;
        Assert.Equal("Retried title", result.AudioBookInfo.Name);
        Assert.Equal("Retried description", result.AudioBookInfo.Description);
        result.AudioBookInfo.Name = "Detached title";
        await books.UpdateAsync(result, timeout.Token);
        Assert.Equal("Detached title", (await books.GetByIdAsync(result.Id))!.AudioBookInfo.Name);
        await books.DeleteByIdAsync(result.Id);
    }
    private static string Text(CallToolResult result) => string.Join(" ", result.Content.OfType<TextContentBlock>().Select(t => t.Text));
    private static void AssertError(CallToolResult result, string code)
    {
        Assert.True(result.IsError, Text(result));
        Assert.Equal(code, result.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString());
    }
}
