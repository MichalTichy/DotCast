using DotCast.Infrastructure.Persistence;
namespace DotCast.Infrastructure.PersonalApiTokens.Models;
public sealed class PersonalApiToken : IItemWithId
{
    public required string Id { get; init; }
    public required string OwnerId { get; init; }
    public required string Name { get; init; }
    public required byte[] SecretHash { get; init; }
    public required bool CanWrite { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? RevokedAt { get; set; }
}
