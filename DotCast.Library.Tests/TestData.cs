using DotCast.SharedKernel.Models;

namespace DotCast.Library.Tests
{
    internal static class TestData
    {
        public const string UserId = "user";
        public static readonly DateTime Now = new(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);

        public static AudioBook Book(string id, int rating = 50, string? series = null, int order = 0)
        {
            return new AudioBook
            {
                Id = id,
                LibraryId = "library",
                Rating = rating,
                AudioBookInfo = new AudioBookInfo
                {
                    Id = id,
                    Name = id,
                    AuthorName = "Author",
                    Chapters = [],
                    SeriesName = series,
                    OrderInSeries = order
                }
            };
        }

        /// <summary>Playback whose feed was polled once a day from <paramref name="firstPoll"/> to <paramref name="lastPoll"/> and which downloaded a file.</summary>
        public static AudioBookPlayback PolledDaily(string audioBookId, DateTime firstPoll, DateTime lastPoll, bool finalFile = false)
        {
            var playback = AudioBookPlayback.Create(audioBookId, UserId);
            for (var poll = firstPoll; poll <= lastPoll; poll = poll.AddDays(1))
            {
                playback.RegisterRssGenerated(poll);
            }

            playback.RegisterFileDownloaded(firstPoll.AddHours(1), finalFile);
            return playback;
        }

        public static AudioBookPlayback Finished(string audioBookId, DateTime finishedAt)
        {
            var playback = AudioBookPlayback.Create(audioBookId, UserId);
            playback.RegisterRssGenerated(finishedAt.AddDays(-10));
            playback.MarkFinished(finishedAt);
            return playback;
        }

        public static UserAudioBook Rated(string audioBookId, int rating)
        {
            var userAudioBook = UserAudioBook.Create(audioBookId, UserId);
            userAudioBook.Rate(rating, Now);
            return userAudioBook;
        }

        public static UserAudioBook NotInterested(string audioBookId)
        {
            var userAudioBook = UserAudioBook.Create(audioBookId, UserId);
            userAudioBook.MarkNotInterested(Now);
            return userAudioBook;
        }
    }
}
