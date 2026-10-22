using DotCast.Library.Featured;
using DotCast.Library.Playback;
using DotCast.SharedKernel.Models;
using FluentAssertions;
using static DotCast.Library.Tests.TestData;

namespace DotCast.Library.Tests
{
    public class FeaturedAudioBookSelectorTests
    {
        private static readonly FinishedListeningOptions Options = new();

        private static FeaturedAudioBook? Select(
            IReadOnlyCollection<AudioBook> library,
            IEnumerable<AudioBookPlayback>? playbacks = null,
            IEnumerable<UserAudioBook>? userAudioBooks = null)
        {
            var context = AudioBookUserContext.Create(playbacks ?? [], userAudioBooks ?? []);
            return FeaturedAudioBookSelector.Select(library, library, context, Now, Options);
        }

        [Fact]
        public void WithoutHistory_PicksTopRatedUnplayed()
        {
            var library = new[] { Book("low", 40), Book("high", 90), Book("mid", 70) };

            var result = Select(library);

            result!.AudioBook.Id.Should().Be("high");
            result.Reason.Should().Be(FeaturedAudioBookReason.TopRated);
        }

        [Fact]
        public void NextInSeries_TakesPrecedenceOverTopRated()
        {
            var library = new[] { Book("top", 99), Book("saga-1", 60, "Saga", 1), Book("saga-2", 50, "Saga", 2) };

            var result = Select(library, [Finished("saga-1", Now.AddDays(-1))]);

            result!.AudioBook.Id.Should().Be("saga-2");
            result.Reason.Should().Be(FeaturedAudioBookReason.NextInSeries);
            result.ContinuesAudioBookName.Should().Be("saga-1");
        }

        [Fact]
        public void NextInSeries_UsesMostRecentlyListenedBookFirst()
        {
            var library = new[]
            {
                Book("a-1", 50, "A", 1), Book("a-2", 50, "A", 2),
                Book("b-1", 50, "B", 1), Book("b-2", 50, "B", 2)
            };

            var result = Select(library, [Finished("a-1", Now.AddDays(-5)), Finished("b-1", Now.AddDays(-1))]);

            result!.AudioBook.Id.Should().Be("b-2");
        }

        [Fact]
        public void NextInSeries_SkipsGapsAndStartedOrDismissedBooks()
        {
            var library = new[]
            {
                Book("s-1", 50, "Series", 1),
                Book("s-2", 50, "Series", 2),
                Book("s-4", 50, "Series", 4),
                Book("s-5", 50, "Series", 5),
                Book("other", 90)
            };
            var started = AudioBookPlayback.Create("s-2", UserId);
            started.RegisterFileDownloaded(Now.AddDays(-1), false);

            var result = Select(library, [Finished("s-1", Now.AddDays(-2)), started], [NotInterested("s-4")]);

            result!.AudioBook.Id.Should().Be("s-5");
        }

        [Fact]
        public void SeriesName_IsMatchedIgnoringDiacriticsAndCase()
        {
            var library = new[] { Book("z-1", 50, "Zaklínač", 1), Book("z-2", 50, "zaklinac", 2), Book("top", 99) };

            var result = Select(library, [Finished("z-1", Now.AddDays(-1))]);

            result!.AudioBook.Id.Should().Be("z-2");
        }

        [Fact]
        public void OnlyTenMostRecentlyListenedBooksAreConsidered()
        {
            var library = new List<AudioBook> { Book("old-1", 50, "Old", 1), Book("old-2", 50, "Old", 2), Book("top", 90) };
            var playbacks = new List<AudioBookPlayback> { Finished("old-1", Now.AddDays(-30)) };
            for (var i = 0; i < FeaturedAudioBookSelector.RecentlyListenedCount; i++)
            {
                library.Add(Book($"recent-{i}", 10));
                playbacks.Add(Finished($"recent-{i}", Now.AddDays(-i)));
            }

            var result = Select(library, playbacks);

            result!.AudioBook.Id.Should().Be("top");
        }

        [Fact]
        public void ExcludesListenedRatedAndNotInterestedBooks()
        {
            var library = new[] { Book("listened", 99), Book("rated", 98), Book("dismissed", 97), Book("left", 10) };

            var result = Select(
                library,
                [Finished("listened", Now.AddDays(-1))],
                [Rated("rated", 7), NotInterested("dismissed")]);

            result!.AudioBook.Id.Should().Be("left");
        }

        [Fact]
        public void FeedOnlyOpened_StillCountsAsUnplayed()
        {
            var opened = AudioBookPlayback.Create("opened", UserId);
            opened.RegisterRssGenerated(Now.AddDays(-1));

            var result = Select([Book("opened", 99), Book("other", 10)], [opened]);

            result!.AudioBook.Id.Should().Be("opened");
        }

        [Fact]
        public void NothingAvailable_ReturnsNull()
        {
            Select([Book("dismissed")], userAudioBooks: [NotInterested("dismissed")]).Should().BeNull();
        }

        [Fact]
        public void RespectsCandidateSubset()
        {
            var library = new[] { Book("s-1", 50, "S", 1), Book("s-2", 50, "S", 2), Book("filtered", 10) };
            var context = AudioBookUserContext.Create([Finished("s-1", Now.AddDays(-1))], []);

            var result = FeaturedAudioBookSelector.Select(library, [library[2]], context, Now, Options);

            result!.AudioBook.Id.Should().Be("filtered");
        }
    }
}
