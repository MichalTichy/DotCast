using System.Text.Json;
using DotCast.Library.Mcp.Texts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
namespace DotCast.Library.Mcp.Tools;
public sealed class ToolResults(IHttpContextAccessor accessor, ILogger<ToolResults> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public async Task<CallToolResult> RunAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken, bool providerOperation = false)
    {
        try
        {
            var value = await operation();
            var content = JsonSerializer.SerializeToElement(value, JsonOptions);
            return new CallToolResult { Content = [new TextContentBlock { Text = content.GetRawText() }], StructuredContent = content };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            var code = exception switch {
                UnauthorizedAccessException => "access_denied",
                KeyNotFoundException => "not_found",
                ArgumentException or JsonException => "invalid_input",
                _ => providerOperation ? "provider_unavailable" : "internal_error"
            };
            var correlationId = accessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            if (code is "internal_error" or "provider_unavailable")
                logger.LogError(exception, "MCP operation failed; correlation {CorrelationId}", correlationId);
            var content = JsonSerializer.SerializeToElement(new { error = new { code, message = McpTexts.Get(code), correlationId } }, JsonOptions);
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = content.GetRawText() }], StructuredContent = content };
        }
    }
}
