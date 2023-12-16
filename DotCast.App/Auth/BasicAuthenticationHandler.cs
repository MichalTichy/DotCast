using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace DotCast.App.Auth
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IAuthenticationManager authenticationManager;
        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Basic";
            return base.HandleChallengeAsync(properties);
        }

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,IAuthenticationManager authenticationManager)
            : base(options, logger, encoder)
        {
            this.authenticationManager = authenticationManager;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthenticationResult result;

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]!);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];
                result = authenticationManager.VerifyCredentials(username, password);
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }

            if (!result.Success)
                return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
            var claims = new[]
            {
                new(ClaimTypes.Name, result.Name!),
                new Claim(ClaimTypes.Role, result.IsAdmin ? "Admin" : "User")
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

    }
}
