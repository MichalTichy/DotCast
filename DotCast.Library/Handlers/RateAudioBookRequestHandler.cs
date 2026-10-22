using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Infrastructure.Persistence.Marten.UnitOfWorks;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class RateAudioBookRequestHandler(
        ICurrentUserIdProvider currentUserIdProvider,
        IReadOnlyRepository<AudioBook> audioBookRepository,
        INoTenancyRepository<UserAudioBook> userAudioBookRepository,
        INoTenancyReadOnlyRepository<AudioBookPlayback> playbackRepository,
        IMessagePublisher messenger)
        : IMessageHandler<RateAudioBookRequest>
    {
        public async Task Handle(RateAudioBookRequest message)
        {
            if (!UserAudioBook.IsValidRating(message.Rating))
            {
                throw new ArgumentOutOfRangeException(nameof(message.Rating), message.Rating,
                    $"Rating must be between {UserAudioBook.MinRating} and {UserAudioBook.MaxRating}.");
            }

            var userId = await currentUserIdProvider.GetCurrentUserIdRequiredAsync();
            await audioBookRepository.GetRequiredByIdAsync(message.AudioBookId);

            await using var unitOfWork = new UnitOfWorkProvider();
            var now = DateTime.UtcNow;
            var userAudioBook = await userAudioBookRepository.GetByIdAsync(UserAudioBook.BuildId(message.AudioBookId, userId))
                                ?? UserAudioBook.Create(message.AudioBookId, userId);
            userAudioBook.Rate(message.Rating, now);
            await userAudioBookRepository.UpsertAsync(userAudioBook);

            var playback = await playbackRepository.GetByIdAsync(AudioBookPlayback.BuildId(message.AudioBookId, userId));
            if (playback?.Status != PlaybackStatus.Finished)
            {
                await messenger.ExecuteAsync(new AudioBookPlaybackMarkedFinished(message.AudioBookId, userId, now));
            }

            await unitOfWork.CommitAsync();
        }
    }
}
