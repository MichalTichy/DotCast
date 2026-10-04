namespace DotCast.Library.Mcp.Models;
public sealed record AudioBookSummary(string Id, string LibraryId, string Title, string Author, string? Series, int PositionInSeries, int Rating);
