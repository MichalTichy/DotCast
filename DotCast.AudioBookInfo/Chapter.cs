namespace DotCast.AudioBookInfo
{
    public class Chapter
    {
        public required string Name { get; init; }
        public required string Url { get; init; }
        public required TimeSpan Duration { get; init; }
    }
}