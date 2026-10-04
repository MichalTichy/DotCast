using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using DotCast.Infrastructure.PersonalApiTokens.Models;
namespace DotCast.Infrastructure.PersonalApiTokens.Authentication;
public static class PersonalTokenCredential
{
    public static (string Credential, string Id, byte[] Hash) Generate()
    {
        var id = Guid.NewGuid().ToString("N");
        var secret = RandomNumberGenerator.GetBytes(32);
        return ($"dcpat_v1.{id}.{WebEncoders.Base64UrlEncode(secret)}", id, SHA256.HashData(secret));
    }
    public static bool TryParse(string? credential, out string id, out byte[] hash)
    {
        id = string.Empty;
        hash = [];
        if (credential is null || credential.Length != 85) return false;
        var parts = credential.Split('.');
        if (parts.Length != 3 || parts[0] != "dcpat_v1" ||
            parts[1].Length != 32 || !Guid.TryParseExact(parts[1], "N", out _) || parts[2].Length != 43 ||
            parts[2].Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-' && c != '_')) return false;
        try
        {
            var secret = WebEncoders.Base64UrlDecode(parts[2]);
            if (secret.Length != 32 || WebEncoders.Base64UrlEncode(secret) != parts[2]) return false;
            id = parts[1];
            hash = SHA256.HashData(secret);
            return true;
        }
        catch (FormatException) { return false; }
    }
    public static bool IsValid(PersonalApiToken token, byte[] hash, DateTimeOffset now) =>
        token.RevokedAt is null && token.ExpiresAt > now &&
        token.SecretHash.Length == 32 && CryptographicOperations.FixedTimeEquals(token.SecretHash, hash);
}
