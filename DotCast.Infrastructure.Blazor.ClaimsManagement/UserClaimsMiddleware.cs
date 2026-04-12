using Microsoft.AspNetCore.Http;

namespace DotCast.Infrastructure.Blazor.ClaimsManagement;

public class UserClaimsMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context, IHttpContextUserInfoSetter contextUserInfoSetter)
    {
        contextUserInfoSetter.LoadUserInfo();
        await next(context);
    }
}
