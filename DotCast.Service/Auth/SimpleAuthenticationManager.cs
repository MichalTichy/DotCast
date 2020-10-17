using DotCast.Service.Settings;

namespace DotCast.Service.Auth
{
    public class SimpleAuthenticationManager : IAuthenticationManager
    {
        private readonly AuthenticationSettings authenticationSettings;

        public SimpleAuthenticationManager(AuthenticationSettings authenticationSettings)
        {
            this.authenticationSettings = authenticationSettings;
        }
        public bool VerifyCredentials(string username, string password)
        {
            return username == authenticationSettings.Username && password == authenticationSettings.Password;
        }
    }
}