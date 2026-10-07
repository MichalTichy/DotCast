using DotCast.SharedKernel.Models;

namespace DotCast.Library.Playback
{
    public static class FinishedListeningEstimator
    {
        public static LikelyFinishedEvaluation Evaluate(AudioBookPlayback playback, DateTime nowUtc, FinishedListeningOptions options)
        {
            if (playback.Status == PlaybackStatus.Finished)
            {
                return new LikelyFinishedEvaluation(true, "Marked as listened");
            }

            var lastActivity = playback.LastActivityAt;
            if (playback.NotFinishedConfirmedAt is { } confirmedAt && lastActivity <= confirmedAt)
            {
                return LikelyFinishedEvaluation.No;
            }

            var quiet = nowUtc - lastActivity;
            var quietMin = TimeSpan.FromDays(options.QuietMinDays);

            if (playback.LastFileDownloadedAt.HasValue && TryGetPollingInterval(playback, options, out var interval))
            {
                var requiredQuiet = Max(quietMin, interval * options.QuietIntervalMultiplier);
                if (quiet >= requiredQuiet)
                {
                    return new LikelyFinishedEvaluation(true, $"Feed stopped refreshing {FormatDays(quiet)} ago");
                }
            }

            if (playback.HasDownloadedFinalFile && quiet >= quietMin)
            {
                return new LikelyFinishedEvaluation(true, $"Last chapter downloaded, no activity for {FormatDays(quiet)}");
            }

            return LikelyFinishedEvaluation.No;
        }

        private static bool TryGetPollingInterval(AudioBookPlayback playback, FinishedListeningOptions options, out TimeSpan interval)
        {
            interval = TimeSpan.Zero;
            if (playback.FirstRssGeneratedAt == default)
            {
                return false;
            }

            var span = playback.LastRssGeneratedAt - playback.FirstRssGeneratedAt;
            if (span < TimeSpan.FromDays(options.MinPollSpanDays))
            {
                return false;
            }

            // Records created before poll days were counted have RssPollDayCount == 0; assume daily polling for them.
            if (playback.RssPollDayCount == 0)
            {
                interval = TimeSpan.FromDays(1);
                return true;
            }

            if (playback.RssPollDayCount < options.MinPollDays)
            {
                return false;
            }

            interval = span / (playback.RssPollDayCount - 1);
            return true;
        }

        private static TimeSpan Max(TimeSpan first, TimeSpan second) => first > second ? first : second;

        private static string FormatDays(TimeSpan value)
        {
            var days = (int)Math.Floor(value.TotalDays);
            return days switch
            {
                0 => "less than a day",
                1 => "1 day",
                _ => $"{days} days"
            };
        }
    }
}
