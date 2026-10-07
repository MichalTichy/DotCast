using System.Diagnostics;
using System.Net;
using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.Infrastructure.BookInfoProvider.DatabazeKnih;
using DotCast.Infrastructure.BookInfoProvider.Goodreads;
using DotCast.SharedKernel.Models;
using Xunit;
using Xunit.Abstractions;
namespace DotCast.Library.Mcp.Tests;
public sealed class BookInfoProviderPerformanceTests(ITestOutputHelper output)
{
    private static IBookInfoProvider Provider(bool goodreads, HttpClient http) =>
        goodreads ? new GoodreadsBookInfoProvider(http) : new DatabazeKnihBookInfoProvider(http);
    private static async Task<List<FoundBookInfo>> CollectAsync(IAsyncEnumerable<FoundBookInfo> stream)
    {
        var results = new List<FoundBookInfo>();
        await foreach (var item in stream) results.Add(item);
        return results;
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task DetailRequestsOverlapWithoutChangingSearchOrder(bool goodreads)
    {
        using var pages = new BookPageHttpHandler { DetailDelay = i => TimeSpan.FromMilliseconds(i % 3 == 0 ? 100 : 20) };
        using var http = new HttpClient(pages);
        var items = await CollectAsync(Provider(goodreads, http).GetBookInfoAsync("Book", "Author"));
        Assert.Equal(Enumerable.Range(0, 10).Select(i => $"Book {i}"), items.Select(i => i.Title));
        Assert.Equal(3, pages.MaximumActive);
        Assert.Equal(10, pages.DetailRequests);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task OneRequestedResultLoadsOnlyOneDetailPage(bool goodreads)
    {
        using var pages = new BookPageHttpHandler();
        using var http = new HttpClient(pages);
        Assert.Single(await CollectAsync(Provider(goodreads, http).GetBookInfoAsync("Book", "Author", maxResults: 1)));
        Assert.Equal(1, pages.DetailRequests);
    }
    [Fact]
    public async Task GoodreadsRecognizesTheCurrentSearchLayoutAndRanksAnExactMatchFirst()
    {
        using var pages = new BookPageHttpHandler { ModernGoodreadsSearch = true };
        using var http = new HttpClient(pages);
        var items = await CollectAsync(new GoodreadsBookInfoProvider(http).GetBookInfoAsync("Book 5", "Author", maxResults: 1));
        Assert.Equal("Book 5", Assert.Single(items).Title);
        Assert.Equal(1, pages.DetailRequests);
    }
    [Fact]
    public async Task GoodreadsChallengeReportsAnUnavailableProvider()
    {
        using var pages = new BookPageHttpHandler { SearchStatus = HttpStatusCode.Accepted };
        using var http = new HttpClient(pages);
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            CollectAsync(new GoodreadsBookInfoProvider(http).GetBookInfoAsync("Book", maxResults: 1)));
        Assert.Equal(HttpStatusCode.Accepted, exception.StatusCode);
        Assert.Equal(0, pages.DetailRequests);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task EarlyDisposalCancelsPendingDetails(bool goodreads)
    {
        using var pages = new BookPageHttpHandler { DetailDelay = i => i == 0 ? TimeSpan.FromMilliseconds(20) : TimeSpan.FromSeconds(5) };
        using var http = new HttpClient(pages);
        await foreach (var item in Provider(goodreads, http).GetBookInfoAsync("Book", "Author")) break;
        Assert.Equal(3, pages.DetailRequests);
        Assert.Equal(2, pages.CancelledRequests);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task InvalidMetadataDoesNotUseUpTheRequestedResultCount(bool goodreads)
    {
        using var pages = new BookPageHttpHandler();
        pages.InvalidBooks.Add(0);
        using var http = new HttpClient(pages);
        var items = await CollectAsync(Provider(goodreads, http).GetBookInfoAsync("Book", "Author", maxResults: 2));
        Assert.Equal(new[] { "Book 1", "Book 2" }, items.Select(i => i.Title));
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task CallerCancellationStopsHttpRequests(bool goodreads)
    {
        using var pages = new BookPageHttpHandler { DetailDelay = _ => TimeSpan.FromSeconds(5) };
        using var http = new HttpClient(pages);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CollectAsync(Provider(goodreads, http).GetBookInfoAsync("Book", "Author", cancellation.Token)));
        Assert.True(pages.CancelledRequests > 0);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task ControlledLatencyBenchmark(bool goodreads)
    {
        using var sequentialPages = new BookPageHttpHandler();
        using var sequentialHttp = new HttpClient(sequentialPages);
        var host = goodreads ? "https://www.goodreads.com/book/show/" : "https://www.databazeknih.cz/knihy/";
        var watch = Stopwatch.StartNew();
        for (var i = 0; i < 10; i++) await sequentialHttp.GetStringAsync(host + i);
        var before = watch.Elapsed;
        using var tunedPages = new BookPageHttpHandler();
        using var tunedHttp = new HttpClient(tunedPages);
        watch.Restart();
        var items = await CollectAsync(Provider(goodreads, tunedHttp).GetBookInfoAsync("Book", "Author"));
        var after = watch.Elapsed;
        Assert.Equal(10, items.Count);
        Assert.Equal(3, tunedPages.MaximumActive);
        output.WriteLine($"{(goodreads ? "Goodreads" : "DatabazeKnih")}, 10 details at 80ms simulated network latency: sequential {before.TotalMilliseconds:F0}ms, tuned {after.TotalMilliseconds:F0}ms, ratio {before / after:F2}x");
    }
}
