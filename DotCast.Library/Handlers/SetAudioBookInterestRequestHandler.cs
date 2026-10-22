using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class SetAudioBookInterestRequestHandler(
        ICurrentUserIdProvider currentUserIdProvider,
        IReadOnlyRepository<AudioBook> audioBookRepository,
        INoTenancyRepository<UserAudioBook> userAudioBookRepository)
        : IMessageHandler<SetAudioBookInterestRequest>
    {
        public async Task Handle(SetAudioBookInterestRequest message)
        {
            var userId = await currentUserIdProvider.GetCurrentUserIdRequiredAsync();
            await audioBookRepository.GetRequiredByIdAsync(message.AudioBookId);

            var userAudioBook = await userAudioBookRepository.GetByIdAsync(UserAudioBook.BuildId(message.AudioBookId, userId))
                                ?? UserAudioBook.Create(message.AudioBookId, userId);

            if (message.Interested)
            {
                userAudioBook.ClearNotInterested();
            }
            else
            {
                userAudioBook.MarkNotInterested(DateTime.UtcNow);
            }

            await userAudioBookRepository.UpsertAsync(userAudioBook);
        }
    }
}
