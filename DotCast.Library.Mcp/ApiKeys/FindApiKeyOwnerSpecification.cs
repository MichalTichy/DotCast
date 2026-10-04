using DotCast.Infrastructure.Persistence.Specifications;
using Marten;
namespace DotCast.Library.Mcp.ApiKeys;
public sealed record FindApiKeyOwnerSpecification(string Hash) : ISpecification<AccountApiKey, string>
{
    public async Task<string?> ApplyAsync(IQueryable<AccountApiKey> queryable, CancellationToken cancellationToken = default) =>
        await queryable.Where(k => k.Hash == Hash).Select(k => k.Id).SingleOrDefaultAsync(cancellationToken);
}
