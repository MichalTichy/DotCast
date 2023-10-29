using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DotCast.Infrastructure.BookInfoProvider.Base;

namespace DotCast.Infrastructure.BookInfoProvider.DatabazeKnih
{
    public class DatabazeKnihBookInfoProvider : IBookInfoProvider
    {
        private readonly Uri baseUri = new("https://www.databazeknih.cz/");

        public async IAsyncEnumerable<BookInfo> GetBookInfoAsync(string name)
        {
            await foreach (var foundBook in SearchAsync(name))
            {
                yield return await GetBookInfoAsync(foundBook);
            }
        }

        private async Task<BookInfo> GetBookInfoAsync(BookSearchResult bookSearchResult)
        {
            var page = await LoadPageAsync(bookSearchResult.Url);
            var title = page.QuerySelector("h1[itemprop=\"name\"]")?.TextContent;
            var author = page.QuerySelector("#left_less > div > h2.jmenaautoru > span > a")?.TextContent;
            var description = page.QuerySelector("p[itemprop=\"description\"] >span.start_text")?.TextContent;
            description = RemoveWhitespace(description);
            var seriesName = page.QuerySelector("#bdetail_rest > div.detail_description > h3 > a")?.TextContent;
            var noInSeries = page.QuerySelector("#bdetail_rest > div.detail_description > h3 > em")?.TextContent.TrimEnd('.') ?? "0";
            var imgUrl  = page.QuerySelector("#icover_mid > a > img")?.Attributes["src"]?.Value;
            var rating  = page.QuerySelector("#voixis > a.bpoints > div")?.Text()?.Replace("%","").Trim() ?? "0";
            return new BookInfo(title, author, description, seriesName, int.Parse(noInSeries), imgUrl,int.Parse(rating));
        }

        private string? RemoveWhitespace(string? input)
        {
            return input?.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
        }

        private async IAsyncEnumerable<BookSearchResult> SearchAsync(string bookName)
        {
            var htmlEncodedName = bookName.Replace(" ", "+");
            var searchUrl = new Uri(baseUri, $"/search?q={htmlEncodedName}").ToString();
            var searchPage = await LoadPageAsync(searchUrl);
            var foundBooks = searchPage.QuerySelectorAll("#left_less > p.new");

            foreach (var foundBook in foundBooks)
            {
                var link = foundBook.Children.OfType<IHtmlAnchorElement>().Skip(1).FirstOrDefault();
                if (link?.GetAttribute("type") != "book")
                {
                    continue;
                }
                var title = link?.Text();
                var url = link?.GetAttribute("href");

                if (!string.IsNullOrWhiteSpace(url))
                {
                    url = new Uri(baseUri, url).ToString();
                }
                var additionalInfo = foundBook.Children.OfType<IHtmlSpanElement>().FirstOrDefault()?.TextContent;
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
                {
                    continue;
                }

                yield return new BookSearchResult(title, additionalInfo, url);
            }

        }


        private async Task<IDocument> LoadPageAsync(string url)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(url);
            return document;
        }

    }
}