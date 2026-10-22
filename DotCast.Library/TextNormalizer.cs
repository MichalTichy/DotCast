using System.Globalization;
using System.Text;

namespace DotCast.Library
{
    internal static class TextNormalizer
    {
        public static string NormalizeForSearch(string value)
        {
            var normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static bool AreEquivalent(string? first, string? second)
        {
            return !string.IsNullOrWhiteSpace(first) &&
                   !string.IsNullOrWhiteSpace(second) &&
                   string.Equals(NormalizeForSearch(first), NormalizeForSearch(second), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
