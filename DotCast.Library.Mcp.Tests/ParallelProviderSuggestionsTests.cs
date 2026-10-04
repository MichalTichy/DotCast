using DotCast.BookInfoProvider;
using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.SharedKernel.Messages;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
namespace DotCast.Library.Mcp.Tests;
public sealed class ParallelProviderSuggestionsTests
{
    private static AudiobookInfoSuggestionsRequestHandler Handler(params IBookInfoProvider[] providers) =>
        new(providers, NullLogger<AudiobookInfoSuggestionsRequestHandler>.Instance);
    [Fact]
    public async Task ProvidersStartTogetherAndKeepSourcePreference()
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = new StubBookInfoProvider("First", ct => release.Task.WaitAsync(ct));
        var second = new StubBookInfoProvider("Second");
        var search = Handler(first, second).Handle(new AudiobookInfoSuggestionsRequest("Book"));
        try { await second.Started.Task.WaitAsync(TimeSpan.FromSeconds(2)); }
        finally { release.TrySetResult(); }
        Assert.Equal(new[] { "First 0", "First 1", "Second 0", "Second 1" }, (await search).Select(i => i.Title));
    }
    [Fact]
    public async Task EnoughResultsCancelTheOtherProvider()
    {
        var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stalled = new StubBookInfoProvider("Stalled", async ct => {
            try { await Task.Delay(Timeout.InfiniteTimeSpan, ct); }
            finally { cancelled.TrySetResult(); }
        });
        var result = await Handler(new StubBookInfoProvider("First"), stalled).Handle(new AudiobookInfoSuggestionsRequest("Book", 1));
        Assert.Single(result);
        await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(2));
    }
    [Fact]
    public async Task FailedProviderDoesNotDiscardOtherSuggestions()
    {
        var failed = new StubBookInfoProvider("Failed", _ => Task.FromException(new HttpRequestException("Unavailable")));
        var result = await Handler(failed, new StubBookInfoProvider("Healthy")).Handle(new AudiobookInfoSuggestionsRequest("Book"));
        Assert.Equal(new[] { "Healthy 0", "Healthy 1" }, result.Select(i => i.Title));
    }
    [Fact]
    public async Task AllFailedProvidersReportFailure()
    {
        var failed = new StubBookInfoProvider("Failed", _ => Task.FromException(new HttpRequestException("Unavailable")));
        await Assert.ThrowsAsync<HttpRequestException>(() => Handler(failed).Handle(new AudiobookInfoSuggestionsRequest("Book")));
    }
    [Fact]
    public async Task HttpTimeoutAllowsTheOtherProviderToRespond()
    {
        var timedOut = new StubBookInfoProvider("Timeout", _ => Task.FromException(new TaskCanceledException("HTTP timeout")));
        var result = await Handler(timedOut, new StubBookInfoProvider("Healthy")).Handle(new AudiobookInfoSuggestionsRequest("Book", 1));
        Assert.Single(result);
    }
    [Fact]
    public async Task StalledProviderHasABoundedDeadline()
    {
        var stalled = new StubBookInfoProvider("Stalled", ct => Task.Delay(Timeout.InfiniteTimeSpan, ct));
        var result = await Handler(stalled, new StubBookInfoProvider("Healthy")).Handle(new AudiobookInfoSuggestionsRequest("Book", 1))
            .WaitAsync(TimeSpan.FromSeconds(20));
        Assert.Single(result);
        Assert.Equal("Healthy 0", result.Single().Title);
    }
    [Fact]
    public async Task ZeroResultsDoesNotStartProviders()
    {
        var provider = new StubBookInfoProvider("Unused");
        Assert.Empty(await Handler(provider).Handle(new AudiobookInfoSuggestionsRequest("Book", 0)));
        Assert.False(provider.Started.Task.IsCompleted);
    }
}
