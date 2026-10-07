using System.Globalization;
using System.Resources;

namespace DotCast.App.Shared;

public static class Ux
{
    private static readonly ResourceManager Resources = new("DotCast.App.Shared.UxTexts", typeof(Ux).Assembly);
    public static string Text(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    public static string Format(string key, params object?[] args) => string.Format(CultureInfo.CurrentCulture, Text(key), args);
    public static string Duration(TimeSpan duration) => Format("DurationValue", (int)duration.TotalHours, duration.Minutes);
    public static string Rating(int rating) => rating <= 0 ? Text("Unrated") : Format("RatingValue", rating);
    public static string Titles(int count) => Format(count == 1 ? "OneTitle" : "ManyTitles", count);
}
