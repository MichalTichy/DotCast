using Microsoft.AspNetCore.Http;
namespace DotCast.Infrastructure.PersonalApiTokens.Authentication;
public sealed class PersonalTokenContext(IHttpContextAccessor accessor) : IPersonalTokenContext
{
    public PersonalTokenIdentity RequireScope(string scope)
    {
        var identity = accessor.HttpContext?.Features.Get<PersonalTokenIdentity>();
        if (identity is null || (scope != PersonalTokenDefaults.ReadScope && scope != PersonalTokenDefaults.WriteScope) ||
            (scope == PersonalTokenDefaults.WriteScope && !identity.CanWrite))
            throw new UnauthorizedAccessException("access_denied");
        return identity;
    }
}
