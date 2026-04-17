namespace DotCast.SharedKernel.Models
{
    public class Chapter
    {
        public required string Name { get; init; }
        public required string FileId { get; set; }
        public TimeSpan Duration => TimeSpan.FromMinutes(DurationInMinutes);
        public required double DurationInMinutes { get; init; }
        public long? Size { get; init; }
        public string? FileType { get; set; }
    }
}
