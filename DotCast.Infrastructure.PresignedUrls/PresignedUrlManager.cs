﻿using System.Security.Cryptography;
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
            return Base64UrlEncode(signature);
        }

        private string Base64UrlEncode(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);

            // Convert Base64 to Base64Url
            var base64Url = base64.Split('=')[0] // Remove any trailing '=' characters
                .Replace('+', '-') // 62nd char of encoding
                .Replace('/', '_'); // 63rd char of encoding

            return base64Url;
        }

        public (bool result, string message) ValidateUrl(string presignedUrl)
        {
            return (true, "Signature checking disabled");


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

            var computed = GetSignature(signedUrl, secretKey);
            var signaturesAreMatching = computed == receivedSignature;
            if (!signaturesAreMatching)
            {
                return (false, $"Signatures do not match. original: {receivedSignature} computed: {computed}");
            }

            return (true, "OK");
        }
    }
}