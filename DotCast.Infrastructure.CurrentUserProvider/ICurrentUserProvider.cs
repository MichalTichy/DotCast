namespace DotCast.Infrastructure.CurrentUserProvider;

public interface ICurrentUserProvider<T> where T : class
{
    public Task<T?> GetCurrentUserAsync();
    public Task<T> GetCurrentUserRequiredAsync();
}
