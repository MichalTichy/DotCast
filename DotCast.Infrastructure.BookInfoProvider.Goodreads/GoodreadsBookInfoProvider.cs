using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.SharedKernel.Models;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DotCast.Infrastructure.BookInfoProvider.Goodreads
{
    public class GoodreadsBookInfoProvider : IBookInfoProvider
    {
        private const string SourceName = "Goodreads";
        private static readonly HttpClient httpClient = CreateHttpClient();
        private readonly Uri baseUri = new("https://www.goodreads.com/");
        private readonly Dictionary<string, Category> genreMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Adventure", Category.Adventure },
            { "Biography", Category.BiographiesAndMemoirs },
            { "Biographies", Category.BiographiesAndMemoirs },
            { "Business", Category.EconomicsAndBusiness },
            { "Classics", Category.Novels },
            { "Comic", Category.Comics },
            { "Comics", Category.Comics },
            { "Crime", Category.DetectiveStoriesCrime },
            { "Detective", Category.DetectiveStoriesCrime },
            { "Fantasy", Category.Fantasy },
            { "Fiction", Category.Novels },
            { "Historical Fiction", Category.HistoricalNovels },
            { "History", Category.History },
            { "Horror", Category.Horror },
            { "Humor", Category.Humor },
            { "Mystery", Category.Mysteries },
            { "Nonfiction", Category.FactLiterature },
            { "Philosophy", Category.Philosophy },
            { "Poetry", Category.Poetry },
            { "Politics", Category.PoliticalScienceInternationalRelations },
            { "Psychology", Category.PsychologyAndPedagogy },
            { "Religion", Category.Religion },
            { "Romance", Category.Novels },
            { "Science", Category.Science },
            { "Science Fiction", Category.ScienceFiction },
            { "Self Help", Category.PersonalDevelopmentAndStyle },
            { "Sociology", Category.SociologySociety },
            { "Thriller", Category.Thrillers },
            { "Travel", Category.TravelAndPlaceDescriptions },
            { "War", Category.War },
            { "Young Adult", Category.ForChildrenAndYouth }
        };

        public async IAsyncEnumerable<FoundBookInfo> GetBookInfoAsync(string name, string? author = null)
        {
            var count = 0;
            await foreach (var foundBook in SearchAsync(name, author))
            {
                yield return await GetBookInfoAsync(foundBook.Url);
                count++;
                if (count >= 10)
                {
                    yield break;
                }
            }
        }

        private async Task<FoundBookInfo> GetBookInfoAsync(string url)
        {
            var page = await LoadPageAsync(url);
            var schema = ExtractBookSchema(page);

            var title = CleanTitle(FirstText(page, "[data-testid=\"bookTitle\"]"))
                        ?? CleanTitle(schema?.Title)
                        ?? CleanTitle(GetMetaContent(page, "meta[property=\"og:title\"]"))
                        ?? "ERROR";

            var author = FirstText(page, ".BookPageMetadataSection__contributor [data-testid=\"name\"]", ".ContributorLink__name")
                         ?? schema?.Author
                         ?? "ERROR";

            var description = ExtractDescription(page)
                              ?? GetMetaContent(page, "meta[property=\"og:description\"]")
                              ?? GetMetaContent(page, "meta[name=\"description\"]")
                              ?? schema?.Description;

            var series = ExtractSeries(page, schema?.Title);
            var imageUrl = ToAbsoluteUrl(schema?.ImgUrl ?? GetMetaContent(page, "meta[property=\"og:image\"]"));
            var rating = schema?.PercentageRating ?? ParseRating(FirstText(page, ".RatingStatistics__rating"));
            var categories = ExtractCategories(page);

            return new FoundBookInfo(title, author, description, series.Name, series.Order, imageUrl, rating, categories, SourceName);
        }

        private async IAsyncEnumerable<GoodreadsSearchResult> SearchAsync(string name, string? author)
        {
            var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(author))
            {
                foreach (var result in await SearchSingleQueryAsync($"{name} {author}", name, author, seenUrls))
                {
                    yield return result;
                }
            }

            foreach (var result in await SearchSingleQueryAsync(name, name, author, seenUrls))
            {
                yield return result;
            }
        }

        private async Task<IReadOnlyList<GoodreadsSearchResult>> SearchSingleQueryAsync(
            string query,
            string targetTitle,
            string? targetAuthor,
            HashSet<string> seenUrls)
        {
            var searchUrl = new Uri(baseUri, $"/search?q={Uri.EscapeDataString(query)}&search_type=books").ToString();
            var searchPage = await LoadPageAsync(searchUrl);
            var rows = searchPage.QuerySelectorAll("tr[itemtype=\"http://schema.org/Book\"]");
            var results = new List<GoodreadsSearchResult>();

            foreach (var row in rows)
            {
                var link = row.QuerySelector("a.bookTitle") as IHtmlAnchorElement;
                var title = NormalizeWhitespace(link?.QuerySelector("[itemprop=\"name\"]")?.TextContent ?? link?.TextContent);
                var url = ToAbsoluteUrl(link?.GetAttribute("href"));
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url) || !seenUrls.Add(url))
                {
                    continue;
                }

                var author = NormalizeWhitespace(row.QuerySelector(".authorName [itemprop=\"name\"]")?.TextContent);
                results.Add(new GoodreadsSearchResult(title, author, url));
            }

            return results
                .OrderByDescending(result => ScoreSearchResult(result, targetTitle, targetAuthor))
                .ToList();
        }

        private async Task<IDocument> LoadPageAsync(string url)
        {
            var html = await httpClient.GetStringAsync(url);
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            return await context.OpenAsync(request => request.Content(html).Address(url));
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            return client;
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

            return author.ValueKind == JsonValueKind.String ? NormalizeWhitespace(author.GetString()) : null;
        }

        private static int? ReadPercentageRating(JsonElement root)
        {
            if (!root.TryGetProperty("aggregateRating", out var rating)
                || !rating.TryGetProperty("ratingValue", out var ratingValue))
            {
                return null;
            }

            if (!ratingValue.TryGetDouble(out var parsedRating))
            {
                return null;
            }

            return (int)Math.Round(parsedRating / 5 * 100);
        }

        private static string? ReadString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return NormalizeWhitespace(property.GetString());
        }

        private static string? ExtractDescription(IDocument page)
        {
            var text = page.QuerySelector("[data-testid=\"description\"] .Formatted")?.TextContent
                       ?? page.QuerySelector("[data-testid=\"description\"]")?.TextContent;
            return NormalizeWhitespace(WebUtility.HtmlDecode(text));
        }

        private static (string? Name, int Order) ExtractSeries(IDocument page, string? schemaTitle)
        {
            var seriesLink = page.QuerySelector(".BookPageTitleSection__title a[href*=\"/series/\"]");
            var ariaLabel = seriesLink?.GetAttribute("aria-label");
            var order = ParseFirstInteger(ariaLabel) ?? ParseSeriesOrder(schemaTitle);
            var name = NormalizeWhitespace(seriesLink?.TextContent);
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = Regex.Replace(name, @"\s*#\s*\d+.*$", string.Empty).Trim();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = ParseSeriesName(schemaTitle);
            }

            return (string.IsNullOrWhiteSpace(name) ? null : name, order ?? 0);
        }

        private ICollection<Category> ExtractCategories(IDocument page)
        {
            return page
                .QuerySelectorAll("[data-testid=\"genresList\"] a[href*=\"/genres/\"] .Button__labelItem")
                .Select(element => NormalizeWhitespace(element.TextContent))
                .Where(genre => !string.IsNullOrWhiteSpace(genre))
                .Select(genre => genre!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(genre => genreMap.TryGetValue(genre, out var category) ? category : null)
                .Where(category => category != null)
                .Select(category => category!)
                .DistinctBy(category => category.Name)
                .ToList();
        }

        private string? ToAbsoluteUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            return new Uri(baseUri, WebUtility.HtmlDecode(url)).ToString();
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

        private static string? GetMetaContent(IDocument page, string selector)
        {
            return NormalizeWhitespace(page.QuerySelector(selector)?.GetAttribute("content"));
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
            title = NormalizeWhitespace(title);
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            return Regex.Replace(title, @"\s*\([^)]*#\s*\d+[^)]*\)\s*$", string.Empty).Trim();
        }

        private static int ScoreSearchResult(GoodreadsSearchResult result, string targetTitle, string? targetAuthor)
        {
            var score = 0;
            var candidateTitle = NormalizeForMatch(CleanTitle(result.Title) ?? result.Title);
            var normalizedTitle = NormalizeForMatch(targetTitle);
            if (candidateTitle == normalizedTitle)
            {
                score += 100;
            }
            else if (candidateTitle.Contains(normalizedTitle, StringComparison.Ordinal) || normalizedTitle.Contains(candidateTitle, StringComparison.Ordinal))
            {
                score += 25;
            }

            if (!string.IsNullOrWhiteSpace(targetAuthor) && !string.IsNullOrWhiteSpace(result.Author))
            {
                var candidateAuthor = NormalizeForMatch(result.Author);
                var normalizedAuthor = NormalizeForMatch(targetAuthor);
                if (candidateAuthor == normalizedAuthor || AuthorTokensMatch(candidateAuthor, normalizedAuthor))
                {
                    score += 50;
                }
                else if (candidateAuthor.Contains(normalizedAuthor, StringComparison.Ordinal) || normalizedAuthor.Contains(candidateAuthor, StringComparison.Ordinal))
                {
                    score += 20;
                }
            }

            return score;
        }

        private static bool AuthorTokensMatch(string candidateAuthor, string targetAuthor)
        {
            return targetAuthor
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .All(token => candidateAuthor.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(token));
        }

        private static string NormalizeForMatch(string value)
        {
            var normalized = value.Trim().Normalize(System.Text.NormalizationForm.FormD);
            var builder = new System.Text.StringBuilder(normalized.Length);
            var previousWasWhitespace = false;

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsWhiteSpace(character) || char.IsPunctuation(character))
                {
                    if (!previousWasWhitespace)
                    {
                        builder.Append(' ');
                        previousWasWhitespace = true;
                    }

                    continue;
                }

                builder.Append(char.ToUpperInvariant(character));
                previousWasWhitespace = false;
            }

            return builder.ToString().Trim().Normalize(System.Text.NormalizationForm.FormC);
        }

        private static int ParseRating(string? rating)
        {
            return double.TryParse(rating, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedRating)
                ? (int)Math.Round(parsedRating / 5 * 100)
                : 0;
        }

        private static int? ParseFirstInteger(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var digits = new string(value.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(digits, out var result) ? result : null;
        }

        private static string? ParseSeriesName(string? title)
        {
            var match = Regex.Match(title ?? string.Empty, @"\((?<series>[^,()]+),\s*#\s*\d+");
            return match.Success ? NormalizeWhitespace(match.Groups["series"].Value) : null;
        }

        private static int? ParseSeriesOrder(string? title)
        {
            var match = Regex.Match(title ?? string.Empty, @"#\s*(?<order>\d+)");
            return match.Success && int.TryParse(match.Groups["order"].Value, out var result) ? result : null;
        }

        private sealed record GoodreadsSearchResult(string Title, string? Author, string Url);

        private sealed record BookSchema(string? Title, string? Author, string? ImgUrl, string? Description, int? PercentageRating);
    }
}
