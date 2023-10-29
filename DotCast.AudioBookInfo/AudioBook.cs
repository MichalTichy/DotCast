namespace DotCast.AudioBookInfo
{
    public class AudioBook
    {
        public required string Id { get; init; }
        public required string Name { get; set; }
        public required string AuthorName { get; set; }
        public required List<Chapter> Chapters { get; init; }
        public ICollection<Category> Categories { get; set; }

        public string? SeriesName { get; set; }
        public int OrderInSeries { get; set; }
        public string? Description { get; set; }
        public string Url { get; set; }
        public string? ImageUrl { get; set; }
        public int Rating { get; set; }

        public TimeSpan Duration => TimeSpan.FromMilliseconds(Chapters.Sum(c => c.Duration.TotalMilliseconds));
    }
}