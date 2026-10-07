using DotCast.BookInfoProvider;
using DotCast.SharedKernel.Messages;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
namespace DotCast.Library.Mcp.Tests;
public sealed class MetadataSuggestionsTests
{
    [Fact]
    public async Task SuggestionsStopAtTheRequestedCountAndPassCancellation()
    {
        var provider = new TestBookInfoProvider();
        using var cancellation = new CancellationTokenSource();
        var handler = new AudiobookInfoSuggestionsRequestHandler([provider], NullLogger<AudiobookInfoSuggestionsRequestHandler>.Instance);
        var results = await handler.Handle(new AudiobookInfoSuggestionsRequest("title", 3), cancellation.Token);
        Assert.Equal(3, results.Count);
        Assert.Equal(3, provider.Produced);
        Assert.True(provider.LastCancellationToken.CanBeCanceled);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.Handle(new AudiobookInfoSuggestionsRequest("title", 3), cancellation.Token));
    }
}
