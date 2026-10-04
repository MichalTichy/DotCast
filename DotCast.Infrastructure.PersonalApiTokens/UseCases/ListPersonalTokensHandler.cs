using DotCast.Infrastructure.PersonalApiTokens.Models;
using DotCast.Infrastructure.PersonalApiTokens.Persistence;
namespace DotCast.Infrastructure.PersonalApiTokens.UseCases;
public sealed class ListPersonalTokensHandler(IPersonalApiTokenStore tokens, TokenManagementAccess access)
{
    public async Task<IReadOnlyList<PersonalTokenInfo>> Handle(ListPersonalTokens request, CancellationToken cancellationToken) =>
        await tokens.ListAsync(await access.RequireOwnerAsync(), cancellationToken);
}
