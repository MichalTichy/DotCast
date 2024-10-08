﻿using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.SharedKernel.Models;

namespace DotCast.Infrastructure.BookInfoProvider.DatabazeKnih
{
    public class DatabazeKnihBookInfoProvider : IBookInfoProvider
    {
        private readonly Uri baseUri = new("https://www.databazeknih.cz/");
        private readonly CategoryMapper categoryMapper = new();

        public async IAsyncEnumerable<FoundBookInfo> GetBookInfoAsync(string name)
        {
            var count = 0;
            await foreach (var foundBook in SearchAsync(name))
            {
                count++;
                yield return await GetBookInfoAsync(foundBook);
                if (count >= 10)
                {
                    yield break;
                }
            }
        }

        private async Task<FoundBookInfo> GetBookInfoAsync(BookSearchResult bookSearchResult)
        {
            var page = await LoadPageAsync(bookSearchResult.Url);
            var title = page.QuerySelector("h1[itemprop=\"name\"]")?.TextContent.Replace("přehled", "").Trim();
            var author = page.QuerySelector("#left_less > div > h2.jmenaautoru > span > a")?.TextContent.Trim();
            var description = page.QuerySelector("p[itemprop=\"description\"] >span.start_text")?.TextContent.Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                description = page.QuerySelector("#bdetail_rest > p.justify.new2.odtop")?.TextContent.Trim();
            }

            description = RemoveWhitespace(description);
            var seriesName = page.QuerySelector("#bdetail_rest > div.detail_description > h3 > a")?.TextContent.Trim();
            if (string.IsNullOrWhiteSpace(seriesName))
            {
                seriesName = page.QuerySelector("#bdetail_rest > h3 > a")?.TextContent.Trim();
            }

            var noInSeries = page.QuerySelector("#bdetail_rest > span > span")?.TextContent ?? "0";
            var dotPosition = noInSeries.IndexOf('.');
            if (dotPosition > 0)
            {
                noInSeries = noInSeries.Substring(0, dotPosition).Trim();
            }

            var imgUrl  = page.QuerySelector("#icover_mid > a > img")?.Attributes["src"]?.Value;
            var rating  = page.QuerySelector("#voixis > a.bpoints > div")?.Text()?.Replace("%","").Trim() ?? "0";
            var categoriesRaw = page.QuerySelectorAll("#bdetail_rest > div.detail_description > h5 > a").Select(t => t.Text().Trim()).ToList();

            var categories = new List<Category>(categoriesRaw.Capacity);
            foreach (var rawCategory in categoriesRaw)
            {
                var category = categoryMapper.GetCategoryByCzechName(rawCategory);
                if (category != null)
                {
                    categories.Add(category);
                }
                else
                {
                    Console.WriteLine($"Unknown category: {rawCategory}");
                }
            }

            return new FoundBookInfo(title ?? "ERROR", author ?? "ERROR", description, seriesName, int.Parse(noInSeries), imgUrl, int.Parse(rating), categories);
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
