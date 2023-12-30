using System.Globalization;
using System.Text;

namespace DotCast.Infrastructure.FileNameNormalization
{
    public class FileNameNormalizer : IFileNameNormalizer
    {
        public bool IsNormalized(string path)
        {
            var fileName = Path.GetFileName(path);
            if (fileName.Contains(" "))
            {
                return false;
            }

            if (TryToRemoveDiacritics(fileName, out _))
            {
                return false;
            }

            return true;
        }

        private bool TryToRemoveDiacritics(string input, out string output)
        {
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            var removed = false;
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
                else
                {
                    removed = true; // Diacritic was removed
                }
            }

            output = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            return removed;
        }

        public string Normalize(string path)
        {
            var fileName = Path.GetFileName(path);
            var isFullPath = path != fileName;

            TryToRemoveDiacritics(fileName, out fileName);
            fileName = fileName.Replace(" ", "_");

            if (isFullPath)
            {
                var directory = Path.GetDirectoryName(path)!;
                return Path.Combine(directory, fileName);
            }

            return fileName;
        }
    }
}