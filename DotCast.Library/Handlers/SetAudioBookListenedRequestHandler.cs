using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class SetAudioBookListenedRequestHandler(
        ICurrentUserIdProvider currentUserIdProvider,
        IReadOnlyRepository<AudioBook> audioBookRepository,
        IMessagePublisher messenger)
        : IMessageHandler<SetAudioBookListenedRequest>
    {
        public async Task Handle(SetAudioBookListenedRequest message)
        {
            var userId = await currentUserIdProvider.GetCurrentUserIdRequiredAsync();
            await audioBookRepository.GetRequiredByIdAsync(message.AudioBookId);

            var now = DateTime.UtcNow;
            if (message.Listened)
            {
                await messenger.ExecuteAsync(new AudioBookPlaybackMarkedFinished(message.AudioBookId, userId, now));
            }
            else
            {
                await messenger.ExecuteAsync(new AudioBookPlaybackMarkedUnfinished(message.AudioBookId, userId, now));
            }
        }
    }
}
