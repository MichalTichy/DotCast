using DotCast.AudioBookInfo;

namespace DotCast.AudioBookProvider.Base
{
    public record AudioBooksStatistics(int TotalCount, int AuthorCount, TimeSpan TotalDuration)
    {
        public static AudioBooksStatistics Create(ICollection<AudioBook> audioBooks)
        {
            var audioBookCount = audioBooks.Count;
            var authorCount = audioBooks.Select(t => t.AuthorName).Distinct().Count();
            var totalMinutes = audioBooks.Sum(t => t.Duration.TotalMinutes);
            var totalDuration = TimeSpan.FromMinutes(totalMinutes);
            return new AudioBooksStatistics(audioBookCount, authorCount, totalDuration);
        }
    }
}
