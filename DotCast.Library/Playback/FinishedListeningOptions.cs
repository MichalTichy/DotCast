namespace DotCast.Library.Playback
{
    public class FinishedListeningOptions
    {
        /// <summary>Minimal number of days without any playback signal before a book is considered finished.</summary>
        public double QuietMinDays { get; set; } = 3;

        /// <summary>The quiet period must also be at least this multiple of the usual feed polling interval.</summary>
        public double QuietIntervalMultiplier { get; set; } = 3;

        /// <summary>Minimal number of distinct days the feed was polled to treat polling as regular.</summary>
        public int MinPollDays { get; set; } = 3;

        /// <summary>Minimal span between the first and the last feed poll to treat polling as regular.</summary>
        public double MinPollSpanDays { get; set; } = 2;
    }
}
