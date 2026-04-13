using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class AudioBookDeletedHandler(IRepository<AudioBook> repository) : IMessageHandler<AudioBookDeleted>
    {
        public async Task Handle(AudioBookDeleted message)
        {
            await repository.DeleteByIdAsync(message.Id);
        }
    }
}
