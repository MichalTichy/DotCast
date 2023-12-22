using System.ComponentModel.DataAnnotations;

namespace DotCast.Infrastructure.PresignedUrls
{
    public class PresignedUrlOptions
    {
        [Required]
        public string SecretKey { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int ValidityPeriodInSeconds { get; set; }
    }
}