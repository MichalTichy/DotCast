namespace DotCast.AudioBookProvider.Base
{
    public record AudioBooksStatistics(int TotalCount, int AuthorCount, TimeSpan TotalDuration)
    {
        public static AudioBooksStatistics Create(ICollection<AudioBookInfo> AudioBookInfos)
        {
            var AudioBookCount = AudioBookInfos.Count;
            var authorCount = AudioBookInfos.Select(t => t.AuthorName).Distinct().Count();
            var totalMinutes = AudioBookInfos.Sum(t => t.Duration?.TotalMinutes ?? 0);
            var totalDuration = TimeSpan.FromMinutes(totalMinutes);
            return new AudioBooksStatistics(AudioBookCount, authorCount, totalDuration);
        }
    }
}
