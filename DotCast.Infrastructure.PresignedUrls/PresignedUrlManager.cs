using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;

namespace DotCast.Infrastructure.PresignedUrls
{
    public class PresignedUrlManager(IOptions<PresignedUrlOptions> options) : IPresignedUrlManager
    {
        public string GenerateUrl(string baseUrl, TimeSpan? validity = null)
        {
            var secretKey = options.Value.SecretKey;
            var expiryDateTime = validity != null ? DateTimeOffset.UtcNow.Add(validity.Value) : DateTimeOffset.MaxValue;
            var expiry = expiryDateTime.ToUnixTimeSeconds();

            var uriBuilder = new UriBuilder(baseUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["expiry"] = expiry.ToString();
            uriBuilder.Query = query.ToString();

            var url = uriBuilder.Uri.ToString();
            var signatureText = GetSignature(url, secretKey);
            query["signature"] = signatureText;
            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri.ToString();
        }

        private string GetSignature(string url, string secretKey)
        {
            var stringToSign = url + secretKey;

            var key = Encoding.UTF8.GetBytes(secretKey);
            var data = Encoding.UTF8.GetBytes(stringToSign);

            using var hmacSha256 = new HMACSHA256(key);
            var signature = hmacSha256.ComputeHash(data);

            // Use hex instead of Base64 or Base64-URL
            return HexUrlEncode(signature);
        }

        private string HexUrlEncode(byte[] input)
        {
            // Convert the signature to a lowercase hex string
            // Hex characters (0-9, a-f) are URL-safe by default.
            var hexString = BitConverter.ToString(input).Replace("-", "");
            return hexString.ToLowerInvariant();
        }

        public (bool result, string message) ValidateUrl(string presignedUrl)
        {
            var secretKey = options.Value.SecretKey;

            presignedUrl = FixEscaping(presignedUrl);

            var uri = new Uri(presignedUrl);
            var query = HttpUtility.ParseQueryString(uri.Query);

            var expiry = query["expiry"];
            var receivedSignature = query["signature"];

            if (string.IsNullOrEmpty(expiry) || string.IsNullOrEmpty(receivedSignature))
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
            signedUrl += $"?expiry={expiry}";

            var computed = GetSignature(signedUrl, secretKey);
            var signaturesAreMatching = computed == receivedSignature;
            if (!signaturesAreMatching)
            {
                return (false, $"Signatures do not match. Calculated signature from: {signedUrl}.");
            }

            return (true, "OK");
        }

        private string FixEscaping(string escapedText)
        {
            return escapedText.Replace("&amp;", "&");
        }
    }
}