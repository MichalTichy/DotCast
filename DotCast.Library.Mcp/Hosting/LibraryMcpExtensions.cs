using System.Threading.RateLimiting;
using DotCast.Infrastructure.PersonalApiTokens.Authentication;
using DotCast.Library.Mcp.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
namespace DotCast.Library.Mcp.Hosting;
public static class LibraryMcpExtensions
{
    public static IServiceCollection AddLibraryMcp(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ToolResults>();
        services.AddMcpServer().WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless).WithTools<LibraryTools>();
        services.AddRateLimiter(options => {
            options.RejectionStatusCode = 429;
            options.AddPolicy("library-mcp", context => {
                var token = context.Features.Get<PersonalTokenIdentity>();
                var key = token is null ? $"ip:{context.Connection.RemoteIpAddress}" : $"token:{token.TokenId}";
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions {
                    PermitLimit = token is null ? 20 : 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true });
            });
        });
        return services;
    }
    public static WebApplication UseLibraryMcpBoundary(this WebApplication app)
    {
        app.UseMiddleware<McpRequestBoundaryMiddleware>();
        app.UseRateLimiter();
        return app;
    }
    public static void MapLibraryMcp(this WebApplication app) => app.MapMcp("/mcp")
        .RequireAuthorization(policy => policy.AddAuthenticationSchemes(PersonalTokenDefaults.Scheme).RequireAuthenticatedUser())
        .RequireRateLimiting("library-mcp");
}
