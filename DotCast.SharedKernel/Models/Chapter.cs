using Newtonsoft.Json;

namespace DotCast.SharedKernel.Models
{
    public class Chapter
    {
        public required string Name { get; init; }
        public required string Url { get; init; }
        public TimeSpan Duration => TimeSpan.FromMinutes(DurationInMinutes);
        public required double DurationInMinutes { get; init; }
        public long? Size { get; init; }
    }
}