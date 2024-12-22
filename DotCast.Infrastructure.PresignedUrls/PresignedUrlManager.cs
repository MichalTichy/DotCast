using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Options;

namespace DotCast.Infrastructure.PresignedUrls
{
    public class PresignedUrlManager(IOptions<PresignedUrlOptions> options) : IPresignedUrlManager
    {
        public string GenerateUrl(string baseUrl, TimeSpan? validity = null)
        {
            var secretKey = options.Value.SecretKey;

            // Calculate expiry (Unix timestamp) -- default to max if not specified
            var expiryDateTime = validity.HasValue
                ? DateTimeOffset.UtcNow.Add(validity.Value)
                : DateTimeOffset.MaxValue;
            var expiry = expiryDateTime.ToUnixTimeSeconds();

            // Remove trailing slash from baseUrl
            baseUrl = baseUrl.TrimEnd('/');

            // Create the "partial" URL that includes the expiry but not the signature
            // e.g. https://example.com/path/1703241923
            var partialUrl = $"{baseUrl}/{expiry}";

            // Compute signature for partialUrl
            var signature = GetSignature(partialUrl, secretKey);

            // Append signature so final URL is: https://example.com/path/1703241923-<signature>
            var presignedUrl = $"{partialUrl}-{signature}";

            return presignedUrl;
        }

        public (bool result, string message) ValidateUrl(string presignedUrl)
        {
            var secretKey = options.Value.SecretKey;


            // Parse the incoming Uri
            var uri = new Uri(presignedUrl);

            // The last segment contains both expiry and signature separated by '-'
            // E.g. .../1703241923-2aed9b37b6...
            var lastSegment = uri.Segments[^1]; // e.g. 1703241923-2aed9b37b6...
            // Remove any trailing slash from the segment if it exists
            lastSegment = lastSegment.TrimEnd('/');

            // Split on '-'; we expect two parts: expiry and signature
            var parts = lastSegment.Split(['-'], 2);
            if (parts.Length != 2)
            {
                return (false, $"Invalid last segment. Expected 'expiry-signature' but got '{lastSegment}'");
            }

            var expiryString = parts[0];
            var receivedSignature = parts[1];

            if (string.IsNullOrEmpty(expiryString) || string.IsNullOrEmpty(receivedSignature))
            {
                return (false, "One of required parts is null (expiry or signature).");
            }

            if (!long.TryParse(expiryString, out var expiryUnix))
            {
                return (false, "Unable to parse link expiration as long.");
            }

            // Check if link has expired
            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiryUnix);
            if (expiryDateTime < DateTimeOffset.UtcNow)
            {
                return (false, "Link expired");
            }

            // Rebuild the "partial" URL (without the signature).
            // Example: everything up to the last "/" + expiryString
            //
            // uri.GetLeftPart(UriPartial.Path) includes the trailing slash; we can remove it.
            // Then re-append /<expiryString>
            var rawPath = uri.ToString().Replace(lastSegment, "").TrimEnd('/');
            var partialUrl = $"{rawPath}/{expiryString}";

            // Compute a new signature from partialUrl, compare with the receivedSignature
            var computedSignature = GetSignature(partialUrl, secretKey);
            if (!string.Equals(computedSignature, receivedSignature, StringComparison.Ordinal))
            {
                return (false, $"Signatures do not match. Computed from: '{partialUrl}'.");
            }

            return (true, "OK");
        }

        private static string RemoveHttpOrHttps(string url)
        {
            // Return as is if string is null/empty to avoid unnecessary processing.
            if (string.IsNullOrWhiteSpace(url))
                return url;

            // Pattern that matches "http://" or "https://" at the start of the string
            var pattern = @"^https?:\/\/";

            // Replace it with an empty string
            return Regex.Replace(url, pattern, string.Empty, RegexOptions.IgnoreCase);
        }

        private string GetSignature(string url, string secretKey)
        {
            // Remove protocol (http:// or https://)
            url = RemoveHttpOrHttps(url);

            // The string to sign is the URL (without protocol) plus the secret key
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
            // (0-9, a-f) are URL-safe by default
            var hexString = BitConverter.ToString(input).Replace("-", "");
            return hexString.ToLowerInvariant();
        }
    }
}
