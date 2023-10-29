namespace DotCast.App.Auth
{
    public class SimpleAuthenticationManager : IAuthenticationManager
    {
        private readonly AuthenticationSettings authenticationSettings;

        public SimpleAuthenticationManager(AuthenticationSettings authenticationSettings)
        {
            this.authenticationSettings = authenticationSettings;
        }

        AuthenticationResult IAuthenticationManager.VerifyCredentials(string username, string password)
        {
            var user = authenticationSettings.Users.SingleOrDefault(t => t.Username == username && t.Password == password);
            if (user == null)
            {
                return new AuthenticationResult(false, null, false);
            }

            return new AuthenticationResult(true, username, user.IsAdmin);
        }
    }
}
