using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;

namespace DotCast.Storage.Handlers
{
    public class AudioBookDeletedHandler(IStorage storage) : IMessageHandler<AudioBookDeleted>
    {
        public async Task Handle(AudioBookDeleted message)
        {
            await storage.DeleteAsync(message.Id);
        }
    }
}
