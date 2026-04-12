using Microsoft.AspNetCore.Http;

namespace DotCast.Infrastructure.Blazor.ClaimsManagement;

public class HttpContextUserInfoSetter(IHttpContextAccessor httpContextAccessor, IUserClaimsProvider claimsProvider) : IHttpContextUserInfoSetter
{
    public void LoadUserInfo()
    {
        var user = httpContextAccessor.HttpContext?.User;
        claimsProvider.User = user;
    }
}
