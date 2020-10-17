using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotCast.Service.Auth
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
            UrlEncoder encoder,
            ISystemClock clock,IAuthenticationManager authenticationManager)
            : base(options, logger, encoder, clock)
        {
            this.authenticationManager = authenticationManager;
        }



        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            bool isAuthOk=false;
            string username;

            
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                username = credentials[0];
                var password = credentials[1];
                isAuthOk = authenticationManager.VerifyCredentials(username,password);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
            if (!isAuthOk)
                return AuthenticateResult.Fail("Invalid Username or Password");
            var claims = new Claim[] {
                new Claim(ClaimTypes.Name, username),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

    }
}