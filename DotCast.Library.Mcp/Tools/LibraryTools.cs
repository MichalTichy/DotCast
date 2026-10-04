using System.ComponentModel;
using System.Text.Json;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Library.Mcp.Models;
using DotCast.Library.Mcp.UseCases;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
namespace DotCast.Library.Mcp.Tools;
[McpServerToolType]
public sealed class LibraryTools(IMessagePublisher messenger, ToolResults results)
{
    [McpServerTool(Name = "search_audiobooks", ReadOnly = true, Destructive = false, OpenWorld = false)]
    [Description("Search accessible audiobooks by title, author or series. Results contain stable IDs and pagination. Search is case-insensitive. Defaults to 20 results; maximum 100.")]
    public Task<CallToolResult> SearchAsync(string? search = null, string? category = null, string? libraryId = null, int offset = 0, int limit = 20, CancellationToken cancellationToken = default) =>
        results.RunAsync(async () => {
            return await messenger.RequestAsync<SearchAudioBooks, AudioBookSearchPage>(new(search, category, libraryId, offset, limit), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "get_audiobook", ReadOnly = true, Destructive = false, OpenWorld = false)]
    [Description("Get descriptive metadata and chapter summaries for an accessible audiobook ID.")]
    public Task<CallToolResult> GetAsync(string id, CancellationToken cancellationToken = default) =>
        results.RunAsync(async () => {
            return await messenger.RequestAsync<GetAudioBookMetadata, AudioBookMetadata>(new(id), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "get_metadata_suggestions", ReadOnly = true, Destructive = false, OpenWorld = true)]
    [Description("Look up metadata suggestions from configured book information providers. Does not change books. Maximum 10 suggestions.")]
    public Task<CallToolResult> SuggestionsAsync(string title, string? author = null, int count = 5, CancellationToken cancellationToken = default) =>
        results.RunAsync(async () => {
            if (string.IsNullOrWhiteSpace(title) || title.Length > 200 || author?.Length > 200 || count is < 1 or > 10)
                throw new ArgumentException("invalid_input");
            var items = await messenger.RequestAsync<AudiobookInfoSuggestionsRequest, IReadOnlyCollection<FoundBookInfo>>(new(title, count, author), cancellationToken);
            return new { suggestions = items };
        }, cancellationToken, providerOperation: true);

    [McpServerTool(Name = "update_audiobook_metadata", ReadOnly = false, Destructive = false, OpenWorld = false)]
    [Description("Update metadata in your own library. changes is an object with only title, author, description, series, positionInSeries, categories (known category names), releaseDate (yyyy-MM-dd), imageUrl, rating (0–100). Omitted fields stay unchanged; null clears description, series, releaseDate or imageUrl. IDs, libraries and chapter files cannot be changed.")]
    public Task<CallToolResult> UpdateAsync(string id, Dictionary<string, JsonElement> changes, CancellationToken cancellationToken = default) =>
        results.RunAsync(async () => {
            return await messenger.RequestAsync<UpdateAudioBookMetadata, AudioBookMetadata>(new(id, new(changes)), cancellationToken);
        }, cancellationToken);
}
