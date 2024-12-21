using Microsoft.AspNetCore.Authorization;

namespace DotCast.App.Shared
{
    [AllowAnonymous]
    public class UnauthorizedAppPage : AppComponentBase
    {
    }
}