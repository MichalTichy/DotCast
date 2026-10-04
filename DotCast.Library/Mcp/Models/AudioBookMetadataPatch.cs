using System.Globalization;
using System.Text.Json;
using DotCast.SharedKernel.Models;
namespace DotCast.Library.Mcp.Models;
public sealed record AudioBookMetadataPatch(IReadOnlyDictionary<string, JsonElement> Changes)
{
    private static readonly HashSet<string> Fields = ["title", "author", "description", "series", "positionInSeries", "categories", "releaseDate", "imageUrl", "rating"];
    public void Validate()
    {
        if (Changes.Count == 0) throw new ArgumentException("invalid_input");
        foreach (var (field, value) in Changes)
        {
            if (!Fields.Contains(field)) throw new ArgumentException("invalid_input");
            switch (field)
            {
                case "title": case "author":
                    if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()) || value.GetString()!.Length > 500)
                        throw new ArgumentException("invalid_input");
                    break;
                case "description": case "series":
                    if (value.ValueKind != JsonValueKind.Null && value.ValueKind != JsonValueKind.String) throw new ArgumentException("invalid_input");
                    break;
                case "imageUrl":
                    if (value.ValueKind != JsonValueKind.Null &&
                        (value.ValueKind != JsonValueKind.String || !Uri.TryCreate(value.GetString(), UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https")))
                        throw new ArgumentException("invalid_input");
                    break;
                case "positionInSeries": case "rating":
                    if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number) || number < 0 || (field == "rating" && number > 100))
                        throw new ArgumentException("invalid_input");
                    break;
                case "releaseDate":
                    if (value.ValueKind != JsonValueKind.Null &&
                        (value.ValueKind != JsonValueKind.String || !DateTime.TryParseExact(value.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)))
                        throw new ArgumentException("invalid_input");
                    break;
                case "categories":
                    if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() > Category.GetAll().Count() ||
                        value.EnumerateArray().Any(c => c.ValueKind != JsonValueKind.String ||
                            !Category.GetAll().Any(known => string.Equals(known.Name, c.GetString(), StringComparison.OrdinalIgnoreCase))))
                        throw new ArgumentException("invalid_input");
                    break;
            }
        }
    }
    public void Apply(AudioBook book)
    {
        Validate();
        foreach (var (field, value) in Changes)
        {
            switch (field)
            {
                case "title": book.AudioBookInfo.Name = value.GetString()!.Trim(); break;
                case "author": book.AudioBookInfo.AuthorName = value.GetString()!.Trim(); break;
                case "description": book.AudioBookInfo.Description = value.GetString(); break;
                case "series": book.AudioBookInfo.SeriesName = value.GetString(); break;
                case "positionInSeries": book.AudioBookInfo.OrderInSeries = value.GetInt32(); break;
                case "rating": book.Rating = value.GetInt32(); break;
                case "imageUrl": book.AudioBookInfo.ImageUrl = value.GetString(); break;
                case "releaseDate":
                    book.AudioBookInfo.ReleaseDate = value.ValueKind == JsonValueKind.Null ? null :
                        DateTime.ParseExact(value.GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture); break;
                case "categories":
                    book.AudioBookInfo.Categories = value.EnumerateArray().Select(c => Category.GetAll().First(known =>
                        string.Equals(known.Name, c.GetString(), StringComparison.OrdinalIgnoreCase))).Distinct().ToList(); break;
            }
        }
    }
}
