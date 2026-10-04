using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
namespace DotCast.Library.Mcp.ApiKeys;
public static class ApiKeyCredential
{
    public static string Generate() => "dcak_" + WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    public static bool TryHash(string? key, out string hash)
    {
        hash = string.Empty;
        if (key is null || key.Length != 48 || !key.StartsWith("dcak_", StringComparison.Ordinal) ||
            key.AsSpan(5).ContainsAnyExcept("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"))
            return false;
        hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
        return true;
    }
}
