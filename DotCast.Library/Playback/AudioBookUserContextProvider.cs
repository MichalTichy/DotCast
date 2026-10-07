using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Playback
{
    public class AudioBookUserContextProvider(
        ICurrentUserIdProvider currentUserIdProvider,
        INoTenancyReadOnlyRepository<AudioBookPlayback> playbackRepository,
        INoTenancyReadOnlyRepository<UserAudioBook> userAudioBookRepository) : IAudioBookUserContextProvider
    {
        public async Task<AudioBookUserContext> GetForCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var userId = await currentUserIdProvider.GetCurrentUserIdRequiredAsync();
            var playbacks = await playbackRepository.ListAsync(new PlaybacksByUserSpecification(userId), cancellationToken);
            var userAudioBooks = await userAudioBookRepository.ListAsync(new UserAudioBooksByUserSpecification(userId), cancellationToken);
            return AudioBookUserContext.Create(playbacks, userAudioBooks);
        }
    }
}
