namespace DotCast.SharedKernel.Models
{
    public record ActivePlaybackInfo(
        string AudioBookId,
        string AudioBookName,
        string UserId,
        string UserName,
        PlaybackStatus Status,
        DateTime LastRssGeneratedAt,
        DateTime? LastFileDownloadedAt,
        bool HasDownloadedFinalFile,
        DateTime? FinishedAt);
}
