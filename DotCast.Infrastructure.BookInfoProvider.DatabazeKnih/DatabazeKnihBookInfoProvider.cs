using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.SharedKernel.Models;
using System.Globalization;
using System.Text.Json;

namespace DotCast.Infrastructure.BookInfoProvider.DatabazeKnih
{
    public class DatabazeKnihBookInfoProvider : IBookInfoProvider
    {
        private const string SourceName = "Databáze knih";
        private readonly Uri baseUri = new("https://www.databazeknih.cz/");
        private readonly CategoryMapper categoryMapper = new();

        public async IAsyncEnumerable<FoundBookInfo> GetBookInfoAsync(string name, string? author = null)
        {
            var count = 0;
            await foreach (var foundBook in SearchAsync(name, author))
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
            var schema = ExtractBookSchema(page);

            var title = CleanTitle(schema?.Title)
                        ?? CleanTitle(FirstText(page, "h1[itemprop=\"name\"]", "h1.oddown_five", "h1"))
                        ?? CleanTitle(GetMetaContent(page, "meta[property=\"og:title\"]"))
                        ?? bookSearchResult.Title;

            var author = schema?.Author
                         ?? FirstText(page, ".orangeBoxLight span.author a", "span.author a", ".jmenaautoru a")
                         ?? ExtractAuthorFromPageTitle(page);

            var description = RemoveWhitespace(ExtractDescription(page)
                                               ?? schema?.Description
                                               ?? GetMetaContent(page, "meta[property=\"og:description\"]")
                                               ?? GetMetaContent(page, "meta[name=\"description\"]"));
            description = StripAuthorSuffix(description, author);

            var seriesName = ExtractSeriesName(page);
            var orderInSeries = ExtractOrderInSeries(page);

            var imgUrl = ToAbsoluteUrl(schema?.ImgUrl
                                       ?? FirstAttribute(page, "#icover_mid > a > img", "src")
                                       ?? FirstAttribute(page, "meta[property=\"og:image\"]", "content"));
            var rating = schema?.PercentageRating ?? ParseFirstInteger(FirstText(page, "#voixis .ratValue", ".ratValue", "#voixis"));
            var categoriesRaw = page
                .QuerySelectorAll(".detail_description a.genre, #bdetail_rest > div.detail_description > h5 > a")
                .Select(t => NormalizeWhitespace(t.TextContent))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t!)
                .Distinct()
                .ToList();

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

            return new FoundBookInfo(title, author ?? "ERROR", description, seriesName, orderInSeries, imgUrl, rating, categories, SourceName);
        }

        private static string? RemoveWhitespace(string? input)
        {
            return NormalizeWhitespace(input);
        }

        private static string? ExtractDescription(IDocument page)
        {
            foreach (var selector in new[] { "p[itemprop=\"description\"]", "p.new2.odtop", "p.justify.new2.odtop" })
            {
                foreach (var paragraph in page.QuerySelectorAll(selector))
                {
                    if (paragraph.ClassList.Contains("comment-text"))
                    {
                        continue;
                    }

                    var text = NormalizeWhitespace(TextContentWithoutToggle(paragraph));
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }

            return null;
        }

        private static string TextContentWithoutToggle(INode node)
        {
            if (node is IElement element && element.Matches("a.show_hide_more"))
            {
                return string.Empty;
            }

            if (node.NodeType == NodeType.Text)
            {
                return node.TextContent;
            }

            return string.Concat(node.ChildNodes.Select(TextContentWithoutToggle));
        }

        private static string? ExtractSeriesName(IDocument page)
        {
            foreach (var link in page.QuerySelectorAll("#bdetail_rest .orangeBoxSmall a[href*=\"/serie/\"]"))
            {
                var text = NormalizeWhitespace(link.GetAttribute("title")) ?? NormalizeWhitespace(link.TextContent);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return FirstText(page, "#bdetail_rest > div.detail_description > h3 > a", "#bdetail_rest > h3 > a");
        }

        private static int ExtractOrderInSeries(IDocument page)
        {
            foreach (var seriesBox in page.QuerySelectorAll("#bdetail_rest .orangeBoxSmall"))
            {
                if (seriesBox.QuerySelector("a[href*=\"/serie/\"]") == null)
                {
                    continue;
                }

                var order = ParseFirstInteger(FirstText(seriesBox, ".nowrap span", ".nowrap"));
                if (order > 0)
                {
                    return order;
                }
            }

            return ParseFirstInteger(FirstText(page, "#bdetail_rest > span > span"));
        }

        private async IAsyncEnumerable<BookSearchResult> SearchAsync(string bookName, string? author)
        {
            var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(author))
            {
                await foreach (var result in SearchSingleQueryAsync($"{bookName} {author}", seenUrls))
                {
                    yield return result;
                }
            }

            await foreach (var result in SearchSingleQueryAsync(bookName, seenUrls))
            {
                yield return result;
            }
        }

        private async IAsyncEnumerable<BookSearchResult> SearchSingleQueryAsync(string query, HashSet<string> seenUrls)
        {
            var htmlEncodedName = Uri.EscapeDataString(query);
            var searchUrl = new Uri(baseUri, $"/search?q={htmlEncodedName}").ToString();
            var searchPage = await LoadPageAsync(searchUrl);
            var foundBookLinks = searchPage
                .QuerySelectorAll("a[type=\"book\"], #left_less a[href*=\"/prehled-knihy/\"], #left_less a[href*=\"/knihy/\"]")
                .OfType<IHtmlAnchorElement>();

            foreach (var link in foundBookLinks)
            {
                var href = link.GetAttribute("href");
                if (string.IsNullOrWhiteSpace(href) || !IsBookHref(href))
                {
                    continue;
                }

                var title = NormalizeWhitespace(link.TextContent);
                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                var url = ToAbsoluteUrl(href);
                if (url == null || !seenUrls.Add(url))
                {
                    continue;
                }

                var container = FindAncestor(link, "p.new");
                var additionalInfo = NormalizeWhitespace(container?.QuerySelector("span.pozn")?.TextContent);
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

        private static BookSchema? ExtractBookSchema(IDocument page)
        {
            var json = page.QuerySelector("script[type=\"application/ld+json\"]")?.TextContent;
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    root = root.EnumerateArray().FirstOrDefault(element => ReadString(element, "@type") == "Book");
                    if (root.ValueKind == JsonValueKind.Undefined)
                    {
                        return null;
                    }
                }
                else if (ReadString(root, "@type") != "Book")
                {
                    return null;
                }

                var title = ReadString(root, "name");
                var author = ReadAuthor(root);
                var image = ReadString(root, "image");
                var description = ReadString(root, "description");
                var percentageRating = ReadPercentageRating(root);
                return new BookSchema(title, author, image, description, percentageRating);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string? ReadAuthor(JsonElement root)
        {
            if (!root.TryGetProperty("author", out var author))
            {
                return null;
            }

            if (author.ValueKind == JsonValueKind.String)
            {
                return NormalizeWhitespace(author.GetString());
            }

            if (author.ValueKind == JsonValueKind.Object)
            {
                return ReadString(author, "name");
            }

            if (author.ValueKind == JsonValueKind.Array)
            {
                return author
                    .EnumerateArray()
                    .Select(element => element.ValueKind == JsonValueKind.Object ? ReadString(element, "name") : element.GetString())
                    .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            }

            return null;
        }

        private static int? ReadPercentageRating(JsonElement root)
        {
            if (!root.TryGetProperty("aggregateRating", out var rating))
            {
                return null;
            }

            var ratingValue = ReadString(rating, "ratingValue");
            var bestRating = ReadString(rating, "bestRating") ?? "5";
            if (!double.TryParse(ratingValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedRating)
                || !double.TryParse(bestRating, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedBestRating)
                || parsedBestRating <= 0)
            {
                return null;
            }

            return (int)Math.Round(parsedRating / parsedBestRating * 100);
        }

        private static string? ReadString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return NormalizeWhitespace(property.GetString());
        }

        private static string? FirstText(IDocument page, params string[] selectors)
        {
            foreach (var selector in selectors)
            {
                var text = NormalizeWhitespace(page.QuerySelector(selector)?.TextContent);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return null;
        }

        private static string? FirstText(IElement element, params string[] selectors)
        {
            foreach (var selector in selectors)
            {
                var text = NormalizeWhitespace(element.QuerySelector(selector)?.TextContent);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return null;
        }

        private static string? FirstAttribute(IDocument page, string selector, string attributeName)
        {
            return NormalizeWhitespace(page.QuerySelector(selector)?.GetAttribute(attributeName));
        }

        private static string? GetMetaContent(IDocument page, string selector)
        {
            return NormalizeWhitespace(page.QuerySelector(selector)?.GetAttribute("content"));
        }

        private string? ToAbsoluteUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            return new Uri(baseUri, url).ToString();
        }

        private static string? NormalizeWhitespace(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            return string.Join(" ", input.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        }

        private static string? CleanTitle(string? title)
        {
            title = NormalizeWhitespace(title)?.Replace("přehled", "").Trim();
            const string suffix = " - kniha";
            if (title?.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) == true)
            {
                title = title[..^suffix.Length].Trim();
            }

            return string.IsNullOrWhiteSpace(title) ? null : title;
        }

        private static string? ExtractAuthorFromPageTitle(IDocument page)
        {
            var pageTitle = NormalizeWhitespace(page.Title);
            const string siteSuffix = "| Databáze knih";
            if (string.IsNullOrWhiteSpace(pageTitle) || !pageTitle.Contains(" - "))
            {
                return null;
            }

            var author = pageTitle.Split(" - ", 2)[1].Replace(siteSuffix, "", StringComparison.OrdinalIgnoreCase).Trim();
            return string.IsNullOrWhiteSpace(author) ? null : author;
        }

        private static string? StripAuthorSuffix(string? description, string? author)
        {
            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(author))
            {
                return description;
            }

            var suffix = $" od {author}";
            return description.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? description[..^suffix.Length].Trim()
                : description;
        }

        private static int ParseFirstInteger(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            var digits = new string(value.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(digits, out var result) ? result : 0;
        }

        private static bool IsBookHref(string href)
        {
            return href.Contains("/knihy/", StringComparison.OrdinalIgnoreCase)
                   || href.Contains("/prehled-knihy/", StringComparison.OrdinalIgnoreCase);
        }

        private static IElement? FindAncestor(IElement element, string selector)
        {
            var current = element.ParentElement;
            while (current != null)
            {
                if (current.Matches(selector))
                {
                    return current;
                }

                current = current.ParentElement;
            }

            return null;
        }

        private sealed record BookSchema(string? Title, string? Author, string? ImgUrl, string? Description, int? PercentageRating);
    }
}
