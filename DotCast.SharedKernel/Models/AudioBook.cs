namespace DotCast.SharedKernel.Models
{
    public class AudioBook
    {
        public required string Id { get; init; }
        public required AudioBookInfo AudioBookInfo { get; set; }

        public int Rating { get; set; }
    }
}