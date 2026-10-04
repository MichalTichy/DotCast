using System.Threading.RateLimiting;
using DotCast.Library.Mcp.ApiKeys;
using DotCast.Infrastructure.Persistence.Marten.StorageConfiguration;
using Microsoft.AspNetCore.Authentication;
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
        services.AddScoped<IApiKeyOwnerResolver, ApiKeyOwnerResolver>();
        services.AddTransient<IStorageConfiguration, AccountApiKeyStorageConfiguration>();
        services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyDefaults.Scheme, null);
        services.AddScoped<ToolResults>();
        services.AddMcpServer().WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless).WithTools<LibraryTools>();
        services.AddRateLimiter(options => {
            options.RejectionStatusCode = 429;
            options.AddPolicy("library-mcp", context => {
                var keyId = context.User.FindFirst(ApiKeyDefaults.KeyIdClaim)?.Value;
                var key = keyId is null ? $"ip:{context.Connection.RemoteIpAddress}" : $"key:{keyId}";
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions {
                    PermitLimit = keyId is null ? 20 : 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true });
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
        .RequireAuthorization(policy => policy.AddAuthenticationSchemes(ApiKeyDefaults.Scheme).RequireAuthenticatedUser())
        .RequireRateLimiting("library-mcp");
}
