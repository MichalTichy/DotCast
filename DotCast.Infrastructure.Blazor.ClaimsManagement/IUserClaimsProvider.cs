using System.Security.Claims;

namespace DotCast.Infrastructure.Blazor.ClaimsManagement;

public interface IUserClaimsProvider
{
    ClaimsPrincipal? User { get; set; }
}
