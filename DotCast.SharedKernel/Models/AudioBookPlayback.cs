using DotCast.Infrastructure.Persistence;

namespace DotCast.SharedKernel.Models
{
    public class AudioBookPlayback : IItemWithId
    {
        public required string Id { get; init; }
        public required string AudioBookId { get; init; }
        public required string UserId { get; init; }

        public PlaybackStatus Status { get; set; } = PlaybackStatus.InfoRetrieved;
        public DateTime FirstRssGeneratedAt { get; set; }
        public DateTime LastRssGeneratedAt { get; set; }
        public DateTime? LastFileDownloadedAt { get; set; }
        public DateTime? LastArchiveReadAt { get; set; }
        public bool HasDownloadedFinalFile { get; set; }
        public DateTime? FinishedAt { get; set; }

        public static string BuildId(string audioBookId, string userId) => $"{audioBookId}:{userId}";

        public static AudioBookPlayback Create(string audioBookId, string userId)
        {
            return new AudioBookPlayback
            {
                Id = BuildId(audioBookId, userId),
                AudioBookId = audioBookId,
                UserId = userId
            };
        }

        public void RegisterRssGenerated(DateTime timestampUtc)
        {
            if (Status != PlaybackStatus.Finished && Status != PlaybackStatus.InProgress && Status != PlaybackStatus.CloseToFinished)
            {
                Status = PlaybackStatus.InfoRetrieved;
            }

            if (FirstRssGeneratedAt == default)
            {
                FirstRssGeneratedAt = timestampUtc;
            }

            LastRssGeneratedAt = timestampUtc;
        }

        public void RegisterFileDownloaded(DateTime timestampUtc, bool isFinalFile)
        {
            if (Status == PlaybackStatus.Finished)
            {
                LastFileDownloadedAt = timestampUtc;
                HasDownloadedFinalFile |= isFinalFile;
                return;
            }

            LastFileDownloadedAt = timestampUtc;

            if (isFinalFile)
            {
                HasDownloadedFinalFile = true;
                Status = PlaybackStatus.CloseToFinished;
                return;
            }

            if (Status == PlaybackStatus.InfoRetrieved)
            {
                Status = PlaybackStatus.InProgress;
                return;
            }

            if (FirstRssGeneratedAt == default)
            {
                Status = PlaybackStatus.InProgress;
            }
        }

        public void RegisterArchiveRead(DateTime timestampUtc)
        {
            LastArchiveReadAt = timestampUtc;
            LastFileDownloadedAt = timestampUtc;
            HasDownloadedFinalFile = true;

            if (Status == PlaybackStatus.Finished)
            {
                return;
            }

            Status = PlaybackStatus.CloseToFinished;
        }

        public void MarkFinished(DateTime timestampUtc)
        {
            Status = PlaybackStatus.Finished;
            FinishedAt = timestampUtc;
        }
    }
}
