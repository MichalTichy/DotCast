namespace DotCast.Infrastructure.PresignedUrls
{
    public interface IPresignedUrlManager
    {
        string GenerateUrl(string baseUrl);
        (bool result, string message) ValidateUrl(string presignedUrl);
    }
}