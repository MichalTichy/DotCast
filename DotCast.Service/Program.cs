using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DotCast.Service.Controllers;
using DotCast.Service.PodcastProviders;
using DotCast.Service.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotCast.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.Configure<PodcastProviderSettings>(configuration.GetSection(nameof(PodcastProviderSettings)));
                    services.AddTransient(provider => provider.GetService< IOptions<PodcastProviderSettings>>().Value);

                    services.AddSingleton<IPodcastProvider, LocalPodcastProvider>();


                    services.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));
                    services.AddTransient(provider => provider.GetService<IOptions<AuthenticationSettings>>().Value);

                    services.AddAuthentication("BasicAuthentication")
                        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
                    services.AddSingleton<IAuthenticationManager,SimpleAuthenticationManager>();


                    services.AddHostedService<DotCastFileManager>();

                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.ConfigureServices(collection =>
                    {
                        collection.AddControllers();
                    });

                    builder.Configure(applicationBuilder =>
                    {
                        applicationBuilder.UseRouting();


                        applicationBuilder.UseAuthentication();
                        applicationBuilder.UseAuthorization();

                        applicationBuilder.UseEndpoints(routeBuilder =>
                        {
                            routeBuilder.MapControllers();
                        });

                    });
                });
    }
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

    public interface IAuthenticationManager
    {
        bool VerifyCredentials(string username, string password);
    }

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

    public class AuthenticationSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
