namespace DotCast.Service.Auth
{
    public interface IAuthenticationManager
    {
        bool VerifyCredentials(string username, string password);
    }
}