using Microsoft.Extensions.Configuration;

namespace DotCast.Infrastructure.Initializer
{
    public abstract class InitializerBase : IInitializer
    {
        public bool DidRun { get; private set; }

        public abstract int Priority { get; }

        public async Task InitializeAsync(IConfiguration configuration)
        {
            if (DidRun)
            {
                throw new InitializerWasAlreadyExecutedException();
            }

            await RunInitializationLogicAsync();

            DidRun = true;
        }

        protected abstract Task RunInitializationLogicAsync();

        public virtual string Name => GetType().Name;
    }
}