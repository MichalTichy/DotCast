using DotCast.Library.Playback;
using DotCast.SharedKernel.Models;
using FluentAssertions;
using static DotCast.Library.Tests.TestData;

namespace DotCast.Library.Tests
{
    public class FinishedListeningEstimatorTests
    {
        private static readonly FinishedListeningOptions Options = new();

        [Fact]
        public void RegularFeedPollingThatStopped_IsLikelyFinished()
        {
            var playback = PolledDaily("book", Now.AddDays(-14), Now.AddDays(-5));

            var result = FinishedListeningEstimator.Evaluate(playback, Now, Options);

            result.IsLikelyFinished.Should().BeTrue();
            result.Reason.Should().Contain("Feed stopped refreshing 5 days ago");
        }

        [Fact]
        public void FeedStillPolled_IsNotFinished()
        {
            var playback = PolledDaily("book", Now.AddDays(-14), Now.AddHours(-2));

            FinishedListeningEstimator.Evaluate(playback, Now, Options).IsLikelyFinished.Should().BeFalse();
        }

        [Fact]
        public void FeedPolledOnSingleDay_IsNotFinished()
        {
            var playback = PolledDaily("book", Now.AddDays(-10), Now.AddDays(-10));

            FinishedListeningEstimator.Evaluate(playback, Now, Options).IsLikelyFinished.Should().BeFalse();
        }

        [Fact]
        public void QuietShorterThanUsualPollingInterval_IsNotFinished()
        {
            // Polled every 3 days -> required quiet is 3 x 3 = 9 days.
            var playback = AudioBookPlayback.Create("book", UserId);
            for (var poll = Now.AddDays(-20); poll <= Now.AddDays(-6); poll = poll.AddDays(3))
            {
                playback.RegisterRssGenerated(poll);
            }

            playback.RegisterFileDownloaded(Now.AddDays(-20), false);

            FinishedListeningEstimator.Evaluate(playback, Now, Options).IsLikelyFinished.Should().BeFalse();
            FinishedListeningEstimator.Evaluate(playback, Now.AddDays(4), Options).IsLikelyFinished.Should().BeTrue();
        }

        [Fact]
        public void FeedWithoutDownloadedFile_IsNotFinished()
        {
            var playback = AudioBookPlayback.Create("book", UserId);
            for (var poll = Now.AddDays(-14); poll <= Now.AddDays(-5); poll = poll.AddDays(1))
            {
                playback.RegisterRssGenerated(poll);
            }

            FinishedListeningEstimator.Evaluate(playback, Now, Options).IsLikelyFinished.Should().BeFalse();
        }

        [Fact]
        public void LegacyRecordWithoutPollDayCount_UsesPollingSpan()
        {
            var playback = AudioBookPlayback.Create("book", UserId);
            playback.FirstRssGeneratedAt = Now.AddDays(-20);
            playback.LastRssGeneratedAt = Now.AddDays(-6);
            playback.LastFileDownloadedAt = Now.AddDays(-7);
            playback.Status = PlaybackStatus.InProgress;

            FinishedListeningEstimator.Evaluate(playback, Now, Options).IsLikelyFinished.Should().BeTrue();
        }

        [Fact]
        public void FinalFileDownloadedAndQuiet_IsLikelyFinished()
        {
            var playback = AudioBookPlayback.Create("book", UserId);
            playback.RegisterFileDownloaded(Now.AddDays(-4), true);

            var result = FinishedListeningEstimator.Evaluate(playback, Now, Options);

            result.IsLikelyFinished.Should().BeTrue();
            result.Reason.Should().Contain("Last chapter downloaded");
        }

        [Fact]
        public void FinalFileDownloadedRecently_IsNotFinished()
        {
            var playback = AudioBookPlayback.Create("book", UserId);
            playback.RegisterFileDownloaded(Now.AddDays(-1), true);

            FinishedListeningEstimator.Evaluate(playback, Now, Options).IsLikelyFinished.Should().BeFalse();
        }

        [Fact]
        public void MarkedFinished_IsFinished()
        {
            var playback = Finished("book", Now.AddMinutes(-1));

            FinishedListeningEstimator.Evaluate(playback, Now, Options).IsLikelyFinished.Should().BeTrue();
        }

        [Fact]
        public void ConfirmedNotFinished_IsIgnoredUntilNewSignal()
        {
            var playback = PolledDaily("book", Now.AddDays(-14), Now.AddDays(-5));
            playback.ConfirmNotFinished(Now.AddDays(-1));

            FinishedListeningEstimator.Evaluate(playback, Now, Options).IsLikelyFinished.Should().BeFalse();

            // User returns to the book and then stops again.
            playback.RegisterRssGenerated(Now.AddDays(1));
            playback.RegisterRssGenerated(Now.AddDays(2));
            FinishedListeningEstimator.Evaluate(playback, Now.AddDays(3), Options).IsLikelyFinished.Should().BeFalse();
            FinishedListeningEstimator.Evaluate(playback, Now.AddDays(8), Options).IsLikelyFinished.Should().BeTrue();
        }
    }
}
