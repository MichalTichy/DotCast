using DotCast.Infrastructure.Persistence;
namespace DotCast.Library.Mcp.ApiKeys;
public sealed class AccountApiKey : IItemWithId
{
    public required string Id { get; init; }
    public required string Hash { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
