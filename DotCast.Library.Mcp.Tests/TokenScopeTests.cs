using DotCast.Infrastructure.PersonalApiTokens.Authentication;
using Microsoft.AspNetCore.Http;
using Xunit;
namespace DotCast.Library.Mcp.Tests;
public sealed class TokenScopeTests
{
    [Fact]
    public void ReadTokenCannotRequestWriteScope()
    {
        var http = new DefaultHttpContext();
        http.Features.Set(new PersonalTokenIdentity("owner", "token", false));
        var context = new PersonalTokenContext(new HttpContextAccessor { HttpContext = http });
        Assert.Equal("owner", context.RequireScope(PersonalTokenDefaults.ReadScope).OwnerId);
        Assert.Throws<UnauthorizedAccessException>(() => context.RequireScope(PersonalTokenDefaults.WriteScope));
        Assert.Throws<UnauthorizedAccessException>(() => context.RequireScope("admin"));
    }
    [Fact]
    public void ClaimsAloneDoNotGrantTokenCapabilities()
    {
        var http = new DefaultHttpContext();
        http.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity("cookie"));
        var context = new PersonalTokenContext(new HttpContextAccessor { HttpContext = http });
        Assert.Throws<UnauthorizedAccessException>(() => context.RequireScope(PersonalTokenDefaults.ReadScope));
    }
}
