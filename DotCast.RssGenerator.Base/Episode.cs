namespace DotCast.RssGenerator.Base
{
    public class Episode
    {
        public string? Title { get; set; }

        public string? SubTitle { get; set; }

        public string? Summary { get; set; }

        public string? Permalink { get; set; }

        public TimeSpan Duration { get; set; }

        public string? Keywords { get; set; }

        public DateTime PublicationDate { get; set; }

        public bool IsExplicit { get; set; }

        public string? FileUrl { get; set; }

        public long FileLength { get; set; }

        public string? FileType { get; set; }
    }
}
