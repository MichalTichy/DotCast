namespace DotCast.Infrastructure.PresignedUrls
{
    public class PresignedUrlOptions
    {
        public string SecretKey { get; set; } = null!;
        public int ValidityPeriodInSeconds { get; set; }
    }
}