using System.Net;
using DotCast.Library.Mcp.ApiKeys;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
namespace DotCast.Library.Mcp.Hosting;
public sealed class McpRequestBoundaryMiddleware(RequestDelegate next, IConfiguration configuration, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/mcp")) { await next(context); return; }
        var allowed = (configuration["Mcp:AllowedHosts"] ?? "localhost;127.0.0.1;[::1]").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (!allowed.Contains(context.Request.Host.Host, StringComparer.OrdinalIgnoreCase))
        { context.Response.StatusCode = 400; return; }
        var loopback = context.Connection.RemoteIpAddress is not null && IPAddress.IsLoopback(context.Connection.RemoteIpAddress) &&
            new[] { "localhost", "127.0.0.1", "[::1]" }.Contains(context.Request.Host.Host, StringComparer.OrdinalIgnoreCase);
        if (!context.Request.IsHttps && !(environment.IsDevelopment() && configuration.GetValue<bool>("Mcp:AllowLoopbackHttp") && loopback))
        { context.Response.StatusCode = 400; return; }
        if (context.Request.ContentLength > 64 * 1024) { context.Response.StatusCode = 413; return; }
        var maxBody = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (maxBody is { IsReadOnly: false }) maxBody.MaxRequestBodySize = 64 * 1024;
        var result = await context.AuthenticateAsync(ApiKeyDefaults.Scheme);
        context.User = result.Succeeded ? result.Principal! : new ClaimsPrincipal();
        try { await next(context); }
        catch (BadHttpRequestException exception) when (exception.StatusCode == 413 && !context.Response.HasStarted)
        { context.Response.StatusCode = 413; }
    }
}
