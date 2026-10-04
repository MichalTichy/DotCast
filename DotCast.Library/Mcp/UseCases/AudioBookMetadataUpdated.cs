namespace DotCast.Library.Mcp.UseCases;
public sealed record AudioBookMetadataUpdated(string Id, string OwnerId, IReadOnlyList<string> ChangedFields);
