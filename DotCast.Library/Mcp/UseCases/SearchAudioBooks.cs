namespace DotCast.Library.Mcp.UseCases;
public sealed record SearchAudioBooks(string? Search = null, string? Category = null, string? LibraryId = null, int Offset = 0, int Limit = 20);
