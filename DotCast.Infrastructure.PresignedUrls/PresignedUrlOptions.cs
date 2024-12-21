using System.ComponentModel.DataAnnotations;

namespace DotCast.Infrastructure.PresignedUrls
{
    public class PresignedUrlOptions
    {
        [Required]
        public string SecretKey { get; set; } = null!;
    }
}