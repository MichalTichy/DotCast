namespace DotCast.Infrastructure.CurrentTenancyProvider;

public interface ICurrentTenancyProvider
{
    Task<string> GetUserTenantAsync();
}
