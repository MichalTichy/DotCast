namespace DotCast.Infrastructure.PersonalApiTokens.Models;
public sealed record PersonalTokenInfo(string Id, string Name, bool CanWrite, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, DateTimeOffset? RevokedAt);
