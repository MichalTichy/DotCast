using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotCast.Infrastructure.Persistence.Marten.UnitOfWorks
{
    public class UnitOfWorkProvider : IAsyncDisposable
    {
        private static readonly AsyncLocal<UnitOfWork> Data = new();

        private readonly Guid id = Guid.NewGuid();
        private bool ownsConnection => Data.Value?.OwnerId == id;

        public UnitOfWorkProvider()
        {
            if (Data.Value == null)
            {
                var context = new UnitOfWork(id);
                Data.Value = context;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (ownsConnection && Data.Value != null)
            {
                await Data.Value.DisposeAsync();
            }
        }

        public UnitOfWork Get()
        {
            return Data.Value!;
        }

        public async Task CommitAsync()
        {
            if (ownsConnection && Data.Value != null)
            {
                await Data.Value.CommitAsync();
            }
        }
    }
}