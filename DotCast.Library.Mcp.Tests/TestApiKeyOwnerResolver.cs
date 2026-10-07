using System.Collections.Concurrent;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.ApiKeys;
namespace DotCast.Library.Mcp.Tests;
public sealed class TestApiKeyOwnerResolver : IApiKeyOwnerResolver
{
    public ConcurrentDictionary<string, UserInfo> Users { get; } = new();
    public HashSet<string> LockedOwners { get; } = [];
    public Task<UserInfo?> FindAsync(string ownerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(!LockedOwners.Contains(ownerId) && Users.TryGetValue(ownerId, out var user) ? user : null);
    }
}
