using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class AudioBookDeleteRequestHandler(IRepository<AudioBook> repository, IMessagePublisher messenger) : IMessageHandler<AudioBookDeleteRequest>
    {
        public async Task Handle(AudioBookDeleteRequest message)
        {
            var audioBook = await repository.GetByIdAsync(message.Id);
            if (audioBook is null)
            {
                throw new KeyNotFoundException($"Audiobook {message.Id} was not found or is not available to the current user.");
            }

            await messenger.ExecuteAsync(new AudioBookDeleted(message.Id));
            await repository.DeleteByIdAsync(message.Id);
        }
    }
}
