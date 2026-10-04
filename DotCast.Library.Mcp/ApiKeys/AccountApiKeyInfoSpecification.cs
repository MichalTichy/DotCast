using DotCast.Infrastructure.Persistence.Specifications;
using Marten;
namespace DotCast.Library.Mcp.ApiKeys;
public sealed record AccountApiKeyInfoSpecification(string OwnerId) : ISpecification<AccountApiKey, ApiKeyInfo>
{
    public async Task<ApiKeyInfo?> ApplyAsync(IQueryable<AccountApiKey> queryable, CancellationToken cancellationToken = default) =>
        await queryable.Where(k => k.Id == OwnerId).Select(k => new ApiKeyInfo(k.CreatedAt)).SingleOrDefaultAsync(cancellationToken);
}
