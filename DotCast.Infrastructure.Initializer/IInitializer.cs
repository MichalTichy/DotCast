using Microsoft.Extensions.Configuration;

namespace DotCast.Infrastructure.Initializer
{
    public interface IInitializer
    {
        int Priority { get; }
        public Task InitializeAsync(IConfiguration configuration);
        string Name { get; }
    }
}
