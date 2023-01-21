namespace DotCast.PodcastProvider.Base
{
    public record PodcastsStatistics(int TotalCount, int AuthorCount, TimeSpan TotalDuration)
    {
        public static PodcastsStatistics Create(ICollection<PodcastInfo> podcastInfos)
        {
            var podcastCount = podcastInfos.Count;
            var authorCount = podcastInfos.Select(t => t.AuthorName).Distinct().Count();
            var totalMinutes = podcastInfos.Sum(t => t.Duration?.TotalMinutes ?? 0);
            var totalDuration = TimeSpan.FromMinutes(totalMinutes);
            return new PodcastsStatistics(podcastCount, authorCount, totalDuration);
        }
    }
}