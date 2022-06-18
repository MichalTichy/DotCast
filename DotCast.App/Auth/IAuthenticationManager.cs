namespace DotCast.App.Auth
{
    public interface IAuthenticationManager
    {
        AuthenticationResult VerifyCredentials(string username, string password);
    }

    public record AuthenticationResult(bool Success, string? Name, bool IsAdmin);
}