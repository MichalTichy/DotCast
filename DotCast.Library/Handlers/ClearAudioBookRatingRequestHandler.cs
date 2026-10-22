using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class ClearAudioBookRatingRequestHandler(
        ICurrentUserIdProvider currentUserIdProvider,
        INoTenancyRepository<UserAudioBook> userAudioBookRepository)
        : IMessageHandler<ClearAudioBookRatingRequest>
    {
        public async Task Handle(ClearAudioBookRatingRequest message)
        {
            var userId = await currentUserIdProvider.GetCurrentUserIdRequiredAsync();
            var userAudioBook = await userAudioBookRepository.GetByIdAsync(UserAudioBook.BuildId(message.AudioBookId, userId));
            if (userAudioBook?.Rating == null)
            {
                return;
            }

            userAudioBook.ClearRating();
            await userAudioBookRepository.UpdateAsync(userAudioBook);
        }
    }
}
