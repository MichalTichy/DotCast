namespace DotCast.Infrastructure.PresignedUrls
{
    public interface IPresignedUrlManager
    {
        string GenerateUrl(string baseUrl, TimeSpan? validity = null);
        (bool result, string message) ValidateUrl(string presignedUrl);
    }
}