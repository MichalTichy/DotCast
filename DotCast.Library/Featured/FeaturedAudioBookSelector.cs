using DotCast.Library.Playback;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Featured
{
    public static class FeaturedAudioBookSelector
    {
        public const int RecentlyListenedCount = 10;

        /// <param name="library">All audiobooks available to the user, used to resolve listened books.</param>
        /// <param name="candidates">Audiobooks that may be featured (typically the library narrowed by the current filter).</param>
        public static FeaturedAudioBook? Select(
            IReadOnlyCollection<AudioBook> library,
            IReadOnlyCollection<AudioBook> candidates,
            AudioBookUserContext context,
            DateTime nowUtc,
            FinishedListeningOptions options)
        {
            var available = candidates
                .Where(book => !IsExcluded(book, context))
                .ToList();

            if (available.Count == 0)
            {
                return null;
            }

            var libraryById = library.ToDictionary(book => book.Id);
            var recentlyListened = context.Playbacks.Values
                .Where(playback => FinishedListeningEstimator.Evaluate(playback, nowUtc, options).IsLikelyFinished)
                .OrderByDescending(GetListenedAt)
                .Select(playback => libraryById.GetValueOrDefault(playback.AudioBookId))
                .OfType<AudioBook>()
                .Take(RecentlyListenedCount);

            foreach (var listened in recentlyListened)
            {
                var next = FindNextInSeries(listened, available);
                if (next != null)
                {
                    return new FeaturedAudioBook(next, FeaturedAudioBookReason.NextInSeries, listened.AudioBookInfo.Name);
                }
            }

            var topRated = available
                .OrderByDescending(book => book.Rating)
                .ThenBy(book => book.AudioBookInfo.Name)
                .First();

            return new FeaturedAudioBook(topRated, FeaturedAudioBookReason.TopRated);
        }

        private static bool IsExcluded(AudioBook book, AudioBookUserContext context)
        {
            return context.GetListeningState(book.Id) != ListeningState.Unplayed ||
                   context.GetRating(book.Id).HasValue ||
                   context.IsNotInterested(book.Id);
        }

        private static AudioBook? FindNextInSeries(AudioBook listened, IEnumerable<AudioBook> available)
        {
            var series = listened.AudioBookInfo.SeriesName;
            if (string.IsNullOrWhiteSpace(series))
            {
                return null;
            }

            return available
                .Where(book => TextNormalizer.AreEquivalent(book.AudioBookInfo.SeriesName, series))
                .Where(book => book.AudioBookInfo.OrderInSeries > listened.AudioBookInfo.OrderInSeries)
                .OrderBy(book => book.AudioBookInfo.OrderInSeries)
                .ThenBy(book => book.AudioBookInfo.Name)
                .FirstOrDefault();
        }

        private static DateTime GetListenedAt(AudioBookPlayback playback)
        {
            var finishedAt = playback.FinishedAt ?? DateTime.MinValue;
            return finishedAt > playback.LastActivityAt ? finishedAt : playback.LastActivityAt;
        }
    }
}
