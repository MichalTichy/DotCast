namespace DotCast.Infrastructure.PersonalApiTokens.Authentication;
public interface IPersonalTokenContext
{
    PersonalTokenIdentity RequireScope(string scope);
}
