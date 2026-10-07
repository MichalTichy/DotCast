using System.Globalization;
using System.Text;

namespace DotCast.Library;

internal static class AudioBookId
{
    public static string FromName(string name)
    {
        var normalized = name.Trim().ToLowerInvariant().Replace(" ", "-").Normalize(NormalizationForm.FormD);
        var result = new string(normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        // IDs are also storage paths. Never permit path separators or traversal.
        if (string.IsNullOrWhiteSpace(result) || result.Contains('/') || result.Contains('\\') || result.Contains("..") || result.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Invalid audiobook name.");
        return result.Normalize(NormalizationForm.FormC);
    }
}
