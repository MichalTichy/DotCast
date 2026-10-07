namespace DotCast.Library.Mcp.Models;
public sealed record AudioBookSearchPage(IReadOnlyList<AudioBookSummary> Items, int Total, int Offset, int Limit);
