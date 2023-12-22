namespace DotCast.RssGenerator.Base
{
    public class Feed(string title)
    {
        public string Title { get; set; } = title;

        public string? SubTitle { get; set; }

        public string? Description { get; set; }

        public string? HomepageUrl { get; set; }

        public string? AuthorName { get; set; }

        public string? AuthorEmail { get; set; }

        public string? Copyright { get; set; }

        public string? Language { get; set; }

        public string? ImageUrl { get; set; }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public ICollection<string>? Categories { get; set; }

        public string? TunesCategory { get; set; }

        public string? TunesSubCategory { get; set; }

        public bool IsExplicit { get; set; }

        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
        public TimeSpan Duration => TimeSpan.FromSeconds(Episodes.Sum(t => t.Duration.TotalSeconds));
    }
}
