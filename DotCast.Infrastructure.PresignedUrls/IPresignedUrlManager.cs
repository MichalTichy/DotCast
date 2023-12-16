namespace DotCast.Infrastructure.PresignedUrls
{
    public interface IPresignedUrlManager
    {
        string GenerateUrl(string baseUrl);
        bool ValidateUrl(string presignedUrl);
    }
}