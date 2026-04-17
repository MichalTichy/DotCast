using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;
using DotCast.Infrastructure.UserManagement.Abstractions;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

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

    public class ActivePlaybacksRequestHandler(
        INoTenancyReadOnlyRepository<AudioBookPlayback> playbackRepository,
        INoTenancyReadOnlyRepository<AudioBook> audioBookRepository,
        IUserManager<UserInfo> userManager)
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
                    playback.FinishedAt));
            }

            return result;
        }
    }

    internal static class AudioBookPlaybackRepositoryExtensions
    {
        public static async Task StoreAsync(this INoTenancyRepository<AudioBookPlayback> repository, AudioBookPlayback playback)
        {
            var existing = await repository.GetByIdAsync(playback.Id);
            if (existing == null)
            {
                await repository.AddAsync(playback);
                return;
            }

            await repository.UpdateAsync(playback);
        }
    }
}
