using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class AudioBookNotFinishedRequestHandler(
        ICurrentUserIdProvider currentUserIdProvider,
        IReadOnlyRepository<AudioBook> audioBookRepository,
        IMessagePublisher messenger)
        : IMessageHandler<AudioBookNotFinishedRequest>
    {
        public async Task Handle(AudioBookNotFinishedRequest message)
        {
            var userId = await currentUserIdProvider.GetCurrentUserIdRequiredAsync();
            await audioBookRepository.GetRequiredByIdAsync(message.AudioBookId);
            await messenger.ExecuteAsync(new AudioBookPlaybackNotFinishedConfirmed(message.AudioBookId, userId, DateTime.UtcNow));
        }
    }
}
