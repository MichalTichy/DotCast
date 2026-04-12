using System.Security.Claims;

namespace DotCast.Infrastructure.Blazor.ClaimsManagement;

public class UserClaimsProvider : IUserClaimsProvider
{
    public ClaimsPrincipal? User { get; set; }
}
