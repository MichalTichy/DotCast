using System.Net;
using System.Text.Json;
namespace DotCast.Library.Mcp.Tests;
public sealed class BookPageHttpHandler : HttpMessageHandler
{
    private int active;
    private int maximumActive;
    private int detailRequests;
    private int cancelledRequests;
    public int BookCount { get; init; } = 12;
    public bool ModernGoodreadsSearch { get; init; }
    public HttpStatusCode SearchStatus { get; init; } = HttpStatusCode.OK;
    public Func<int, TimeSpan> DetailDelay { get; init; } = _ => TimeSpan.FromMilliseconds(80);
    public HashSet<int> InvalidBooks { get; } = [];
    public int MaximumActive => maximumActive;
    public int DetailRequests => detailRequests;
    public int CancelledRequests => cancelledRequests;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri!;
        if (uri.AbsolutePath == "/search")
        {
            var goodreads = uri.Host.Contains("goodreads");
            var html = string.Join("", Enumerable.Range(0, BookCount).Select(i => !goodreads ?
                $"<p class='new'><a type='book' href='/knihy/{i}'>Book {i}</a></p>" : ModernGoodreadsSearch ?
                $"<div data-testid='book-list-item'><span data-testid='book-item-title'><a href='/book/show/{i}'>Book {i}</a></span><span data-testid='book-item-contributors'><a><span data-testid='name'>Author</span></a></span></div>" :
                $"<tr itemtype='http://schema.org/Book'><td><a class='bookTitle' href='/book/show/{i}'><span itemprop='name'>Book {i}</span></a><a class='authorName'><span itemprop='name'>Author</span></a></td></tr>"));
            return Page(goodreads ? ModernGoodreadsSearch ? html : "<table>" + html + "</table>" : "<div id='left_less'>" + html + "</div>", SearchStatus);
        }
        var id = int.Parse(uri.AbsolutePath.Split('/').Last());
        Interlocked.Increment(ref detailRequests);
        var running = Interlocked.Increment(ref active);
        int observed;
        do { observed = maximumActive; if (running <= observed) break; }
        while (Interlocked.CompareExchange(ref maximumActive, running, observed) != observed);
        try
        {
            await Task.Delay(DetailDelay(id), cancellationToken);
            var json = JsonSerializer.Serialize(new Dictionary<string, object?> {
                ["@type"] = "Book", ["name"] = InvalidBooks.Contains(id) ? "ERROR" : $"Book {id}",
                ["author"] = new { name = "Author" }, ["description"] = "Fixture description" });
            return Page("<script type='application/ld+json'>" + json + "</script>");
        }
        catch (OperationCanceledException) { Interlocked.Increment(ref cancelledRequests); throw; }
        finally { Interlocked.Decrement(ref active); }
    }
    private static HttpResponseMessage Page(string html, HttpStatusCode status = HttpStatusCode.OK) => new(status) { Content = new StringContent("<html><body>" + html + "</body></html>") };
}
