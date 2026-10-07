using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;
using DotCast.Infrastructure.Persistence.Repositories;
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

            var now = DateTime.UtcNow;
            var userAudioBook = await userAudioBookRepository.GetByIdAsync(UserAudioBook.BuildId(message.AudioBookId, userId))
                                ?? UserAudioBook.Create(message.AudioBookId, userId);
            userAudioBook.Rate(message.Rating, now);
            await userAudioBookRepository.StoreAsync(userAudioBook);

            var playback = await playbackRepository.GetByIdAsync(AudioBookPlayback.BuildId(message.AudioBookId, userId));
            if (playback?.Status != PlaybackStatus.Finished)
            {
                await messenger.PublishAsync(new AudioBookPlaybackMarkedFinished(message.AudioBookId, userId, now));
            }
        }
    }

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

            await userAudioBookRepository.StoreAsync(userAudioBook);
        }
    }

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
                await messenger.PublishAsync(new AudioBookPlaybackMarkedFinished(message.AudioBookId, userId, now));
            }
            else
            {
                await messenger.PublishAsync(new AudioBookPlaybackMarkedUnfinished(message.AudioBookId, userId, now));
            }
        }
    }

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
            await messenger.PublishAsync(new AudioBookPlaybackNotFinishedConfirmed(message.AudioBookId, userId, DateTime.UtcNow));
        }
    }
}
