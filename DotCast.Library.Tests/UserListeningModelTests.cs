using DotCast.Library.Playback;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Models;
using FluentAssertions;
using static DotCast.Library.Tests.TestData;

namespace DotCast.Library.Tests
{
    public class UserListeningModelTests
    {
        [Fact]
        public void RegisterRssGenerated_CountsDistinctDays()
        {
            var playback = AudioBookPlayback.Create("book", UserId);

            playback.RegisterRssGenerated(Now);
            playback.RegisterRssGenerated(Now.AddHours(1));
            playback.RegisterRssGenerated(Now.AddDays(1));

            playback.RssPollDayCount.Should().Be(2);
        }

        [Fact]
        public void MarkUnfinished_RestoresProgressState()
        {
            var playback = AudioBookPlayback.Create("book", UserId);
            playback.RegisterFileDownloaded(Now, true);
            playback.MarkFinished(Now);

            playback.MarkUnfinished();

            playback.Status.Should().Be(PlaybackStatus.CloseToFinished);
            playback.FinishedAt.Should().BeNull();
        }

        [Fact]
        public void ConfirmNotFinished_ReturnsToInProgress()
        {
            var playback = Finished("book", Now.AddDays(-1));

            playback.ConfirmNotFinished(Now);

            playback.Status.Should().Be(PlaybackStatus.InProgress);
            playback.NotFinishedConfirmedAt.Should().Be(Now);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        public void Rate_RejectsOutOfRange(int rating)
        {
            var userAudioBook = UserAudioBook.Create("book", UserId);

            var act = () => userAudioBook.Rate(rating, Now);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void DisplayedRating_PrefersPersonalRating()
        {
            var book = Book("book", 82);

            DisplayedRating.From(book, null).Should().Be(new DisplayedRating(82, false));
            DisplayedRating.From(book, null).ToString().Should().Be("82 %");

            var personal = DisplayedRating.From(book, 7);
            personal.ToString().Should().Be("★ 7/10");
            personal.ToPercent().Should().Be(70);
        }

        [Fact]
        public void RetrievalSpecification_FiltersByListeningState()
        {
            var opened = AudioBookPlayback.Create("opened", UserId);
            opened.RegisterRssGenerated(Now);
            var inProgress = AudioBookPlayback.Create("in-progress", UserId);
            inProgress.RegisterFileDownloaded(Now, false);
            var context = AudioBookUserContext.Create([opened, inProgress, Finished("finished", Now)], []);
            var books = new[] { Book("none"), Book("opened"), Book("in-progress"), Book("finished") };

            Ids(ListeningStateFilter.Unplayed).Should().BeEquivalentTo("none", "opened");
            Ids(ListeningStateFilter.InProgress).Should().BeEquivalentTo("in-progress");
            Ids(ListeningStateFilter.Finished).Should().BeEquivalentTo("finished");
            Ids(ListeningStateFilter.Any).Should().HaveCount(4);

            IEnumerable<string> Ids(ListeningStateFilter state) =>
                new AudioBookRetrievalSpecification(new AudioBookLibraryFilter(ListeningState: state), context)
                    .Apply(books)
                    .Select(book => book.Id);
        }

        [Fact]
        public void RetrievalSpecification_RatingFilterUsesDisplayedRating()
        {
            var context = AudioBookUserContext.Create([], [Rated("personally-loved", 9), Rated("personally-disliked", 3)]);
            var books = new[] { Book("personally-loved", 40), Book("personally-disliked", 95), Book("general-high", 85) };

            var result = new AudioBookRetrievalSpecification(new AudioBookLibraryFilter(MinRating: 80), context)
                .Apply(books)
                .Select(book => book.Id);

            result.Should().BeEquivalentTo("personally-loved", "general-high");
        }
    }
}
