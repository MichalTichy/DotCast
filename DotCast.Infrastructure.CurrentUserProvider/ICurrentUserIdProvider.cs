namespace DotCast.Infrastructure.CurrentUserProvider;

public interface ICurrentUserIdProvider
{
    public Task<string?> GetCurrentUserIdAsync();
    public Task<string> GetCurrentUserIdRequiredAsync();
}
