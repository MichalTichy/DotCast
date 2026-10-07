using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;
using DotCast.Infrastructure.UserManagement.Abstractions;
using DotCast.Library.Playback;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.Extensions.Options;

namespace DotCast.Library.Handlers
{
    public class AudioBookRssGeneratedHandler(INoTenancyRepository<AudioBookPlayback> playbackRepository)
        : IMessageHandler<AudioBookRssGenerated>
    {
        public async Task Handle(AudioBookRssGenerated message)
        {
            var playback = await playbackRepository.GetByIdAsync(AudioBookPlayback.BuildId(message.AudioBookId, message.UserId))
                           ?? AudioBookPlayback.Create(message.AudioBookId, message.UserId);

            playback.RegisterRssGenerated(message.Timestamp);
            await playbackRepository.StoreAsync(playback);
        }
    }

    public class FileReadHandler(
        INoTenancyRepository<AudioBookPlayback> playbackRepository,
        INoTenancyReadOnlyRepository<AudioBook> audioBookRepository)
        : IMessageHandler<FileRead>
    {
        public async Task Handle(FileRead message)
        {
            if (string.IsNullOrWhiteSpace(message.UserId) || string.IsNullOrWhiteSpace(message.FileId))
            {
                return;
            }

            var audioBook = await audioBookRepository.GetByIdAsync(message.AudioBookId);
            if (audioBook == null)
            {
                return;
            }

            var playback = await playbackRepository.GetByIdAsync(AudioBookPlayback.BuildId(message.AudioBookId, message.UserId))
                           ?? AudioBookPlayback.Create(message.AudioBookId, message.UserId);

            var lastChapter = audioBook.AudioBookInfo.Chapters.LastOrDefault();
            var isFinalFile = lastChapter != null &&
                              string.Equals(lastChapter.FileId, message.FileId, StringComparison.OrdinalIgnoreCase);

            playback.RegisterFileDownloaded(message.Timestamp, isFinalFile);
            await playbackRepository.StoreAsync(playback);
        }
    }

    public class ArchiveReadHandler(INoTenancyRepository<AudioBookPlayback> playbackRepository)
        : IMessageHandler<ArchiveRead>
    {
        public async Task Handle(ArchiveRead message)
        {
            if (string.IsNullOrWhiteSpace(message.UserId))
            {
                return;
            }

            var playback = await playbackRepository.GetByIdAsync(AudioBookPlayback.BuildId(message.AudioBookId, message.UserId))
                           ?? AudioBookPlayback.Create(message.AudioBookId, message.UserId);

            playback.RegisterArchiveRead(message.Timestamp);
            await playbackRepository.StoreAsync(playback);
        }
    }

    public class AudioBookPlaybackMarkedFinishedHandler(INoTenancyRepository<AudioBookPlayback> playbackRepository)
        : IMessageHandler<AudioBookPlaybackMarkedFinished>
    {
        public async Task Handle(AudioBookPlaybackMarkedFinished message)
        {
            var playback = await playbackRepository.GetByIdAsync(AudioBookPlayback.BuildId(message.AudioBookId, message.UserId))
                           ?? AudioBookPlayback.Create(message.AudioBookId, message.UserId);

            playback.MarkFinished(message.Timestamp);
            await playbackRepository.StoreAsync(playback);
        }
    }

    public class AudioBookPlaybackMarkedUnfinishedHandler(INoTenancyRepository<AudioBookPlayback> playbackRepository)
        : IMessageHandler<AudioBookPlaybackMarkedUnfinished>
    {
        public async Task Handle(AudioBookPlaybackMarkedUnfinished message)
        {
            var playback = await playbackRepository.GetByIdAsync(AudioBookPlayback.BuildId(message.AudioBookId, message.UserId));
            if (playback == null)
            {
                return;
            }

            playback.MarkUnfinished();
            await playbackRepository.UpdateAsync(playback);
        }
    }

    public class AudioBookPlaybackNotFinishedConfirmedHandler(INoTenancyRepository<AudioBookPlayback> playbackRepository)
        : IMessageHandler<AudioBookPlaybackNotFinishedConfirmed>
    {
        public async Task Handle(AudioBookPlaybackNotFinishedConfirmed message)
        {
            var playback = await playbackRepository.GetByIdAsync(AudioBookPlayback.BuildId(message.AudioBookId, message.UserId))
                           ?? AudioBookPlayback.Create(message.AudioBookId, message.UserId);

            playback.ConfirmNotFinished(message.Timestamp);
            await playbackRepository.StoreAsync(playback);
        }
    }

    public class ActivePlaybacksRequestHandler(
        INoTenancyReadOnlyRepository<AudioBookPlayback> playbackRepository,
        INoTenancyReadOnlyRepository<AudioBook> audioBookRepository,
        IUserManager<UserInfo> userManager,
        IOptions<FinishedListeningOptions> finishedListeningOptions)
        : IMessageHandler<ActivePlaybacksRequest, IReadOnlyList<ActivePlaybackInfo>>
    {
        public async Task<IReadOnlyList<ActivePlaybackInfo>> Handle(ActivePlaybacksRequest message)
        {
            var playbacks = await playbackRepository.ListAsync();
            var active = playbacks
                .Where(p => p.Status != PlaybackStatus.Finished)
                .OrderByDescending(p => p.LastFileDownloadedAt ?? DateTime.MinValue)
                .ThenByDescending(p => p.LastRssGeneratedAt)
                .ToList();

            var now = DateTime.UtcNow;
            var result = new List<ActivePlaybackInfo>(active.Count);
            foreach (var playback in active)
            {
                var audioBook = await audioBookRepository.GetByIdAsync(playback.AudioBookId);
                var user = await userManager.GetUserAsync(playback.UserId);

                result.Add(new ActivePlaybackInfo(
                    playback.AudioBookId,
                    audioBook?.AudioBookInfo.Name ?? playback.AudioBookId,
                    playback.UserId,
                    user?.Name ?? playback.UserId,
                    playback.Status,
                    playback.LastRssGeneratedAt,
                    playback.LastFileDownloadedAt,
                    playback.HasDownloadedFinalFile,
                    playback.FinishedAt,
                    playback.RssPollDayCount,
                    playback.NotFinishedConfirmedAt,
                    FinishedListeningEstimator.Evaluate(playback, now, finishedListeningOptions.Value)));
            }

            return result;
        }
    }
}
