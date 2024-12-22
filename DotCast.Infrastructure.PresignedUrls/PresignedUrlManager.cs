using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Options;

namespace DotCast.Infrastructure.PresignedUrls
{
    public class PresignedUrlManager(IOptions<PresignedUrlOptions> options) : IPresignedUrlManager
    {
        private readonly PresignedUrlOptions options = options.Value;

        public string GenerateUrl(string baseUrl, TimeSpan? validity = null)
        {
            var secretKey = options.SecretKey;

            // Compute expiry (Unix time)
            var expiryDateTime = validity.HasValue
                ? DateTimeOffset.UtcNow.Add(validity.Value)
                : DateTimeOffset.MaxValue;
            var expiry = expiryDateTime.ToUnixTimeSeconds();

            // 1) Build the part that will be signed:
            //    path + "?presignedUrl=" + expiry
            //    (No scheme, no signature in this part)
            var builderForSignature = new UriBuilder(baseUrl)
            {
                Query = "" // clear query so we only have the path
            };
            var pathOnly = builderForSignature.Uri.ToString().TrimEnd('/');
            var urlToSign = pathOnly + "?presignedUrl=" + expiry;

            // 2) Calculate signature
            var signatureText = GetSignature(urlToSign, secretKey);

            // 3) Final URL has a single query param: presignedUrl = <expiry>.<signature>
            var finalBuilder = new UriBuilder(baseUrl)
            {
                Query = $"presignedUrl={expiry}.{signatureText}"
            };

            return finalBuilder.Uri.ToString();
        }

        public (bool result, string message) ValidateUrl(string url)
        {
            var secretKey = options.SecretKey;


            // Parse out the single presignedUrl param
            var uri = new Uri(url);
            var singleParam = HttpUtility.ParseQueryString(uri.Query)["presignedUrl"];
            if (string.IsNullOrEmpty(singleParam))
            {
                return (false, "Query param 'presignedUrl' is missing.");
            }

            // The format we expect is: <expiry>.<signature>
            var parts = singleParam.Split('.');
            if (parts.Length != 2)
            {
                return (false, "Invalid presignedUrl format (must be 'expiry.signature').");
            }

            // Parse expiry
            if (!long.TryParse(parts[0], out var expiryUnix))
            {
                return (false, "Unable to parse link expiration (not a valid Unix timestamp).");
            }

            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiryUnix);

            // Expired?
            if (expiryDateTime < DateTimeOffset.UtcNow)
            {
                return (false, "Link is expired.");
            }

            // Received signature
            var receivedSignature = parts[1];

            // Rebuild the URL we originally signed: path + "?presignedUrl=" + expiry
            var builderForSignature = new UriBuilder(url)
            {
                Query = "" // clear out any existing query
            };
            var pathOnly = builderForSignature.Uri.ToString().TrimEnd('/');
            var urlToSign = pathOnly + "?presignedUrl=" + parts[0];

            // Re-compute the signature
            var computedSignature = GetSignature(urlToSign, secretKey);

            // Compare the signatures
            if (!string.Equals(receivedSignature, computedSignature, StringComparison.Ordinal))
            {
                return (false, "Signatures do not match.");
            }

            return (true, "OK");
        }

        private static string RemoveHttpOrHttps(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            // Pattern that matches "http://" or "https://" at the start
            var pattern = @"^https?:\/\/";
            return Regex.Replace(url, pattern, string.Empty, RegexOptions.IgnoreCase);
        }

        private string GetSignature(string url, string secretKey)
        {
            // Remove scheme from URL (http:// or https://)
            url = RemoveHttpOrHttps(url);

            // Append secretKey and compute HMAC-SHA256
            var stringToSign = url + secretKey;
            using var hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var signatureBytes = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

            return HexUrlEncode(signatureBytes);
        }

        private string HexUrlEncode(byte[] input)
        {
            // Convert the signature to a lowercase hex string
            var hexString = BitConverter.ToString(input).Replace("-", "");
            return hexString.ToLowerInvariant();
        }
    }
}