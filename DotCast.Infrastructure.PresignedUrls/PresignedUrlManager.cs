using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;

namespace DotCast.Infrastructure.PresignedUrls
{
    public class PresignedUrlManager(IOptions<PresignedUrlOptions> options) : IPresignedUrlManager
    {
        public string GenerateUrl(string baseUrl)
        {
            var secretKey = options.Value.SecretKey;
            var validity = TimeSpan.FromSeconds(options.Value.ValidityPeriodInSeconds);
            var expiry = DateTimeOffset.UtcNow.Add(validity).ToUnixTimeSeconds();

            var uriBuilder = new UriBuilder(baseUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["fileId"] = Guid.NewGuid().ToString();
            query["expiry"] = expiry.ToString();
            uriBuilder.Query = query.ToString();

            var stringToSign = uriBuilder.Uri + secretKey;
            var signature = ComputeHmacSha256(Encoding.UTF8.GetBytes(secretKey), Encoding.UTF8.GetBytes(stringToSign));
            query["signature"] = Convert.ToBase64String(signature);
            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri.ToString();
        }

        public (bool result, string message) ValidateUrl(string presignedUrl)
        {
            var secretKey = options.Value.SecretKey;

            var uri = new Uri(presignedUrl);
            var query = HttpUtility.ParseQueryString(uri.Query);

            var fileId = query["fileId"]!;
            var expiry = query["expiry"];
            var receivedSignature = query["signature"];

            if (string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(expiry) || string.IsNullOrEmpty(receivedSignature))
            {
                return (false, "One of required parameters is null.");
            }

            if (!long.TryParse(expiry, out var expiryUnix))
            {
                return (false, "Unable to parse link expiration.");
            }

            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiryUnix);
            if (expiryDateTime < DateTimeOffset.UtcNow)
            {
                return (false, "Link expired");
            }

            var signedUrl = uri.GetLeftPart(UriPartial.Path);
            signedUrl += $"?fileId={fileId}&expiry={expiry}";

            var stringToSign = signedUrl + secretKey;
            var computedSignature = ComputeHmacSha256(Encoding.UTF8.GetBytes(secretKey), Encoding.UTF8.GetBytes(stringToSign));

            var computed = Convert.ToBase64String(computedSignature);
            var signaturesAreMatching = computed == receivedSignature;
            if (!signaturesAreMatching)
            {
                return (false, $"Signatures do not match. original: {presignedUrl} computed: {signedUrl}");
            }

            return (true, "OK");
        }

        private byte[] ComputeHmacSha256(byte[] key, byte[] data)
        {
            using var hmacSha256 = new HMACSHA256(key);
            return hmacSha256.ComputeHash(data);
        }
    }
}