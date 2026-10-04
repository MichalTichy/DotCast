namespace DotCast.Infrastructure.PersonalApiTokens.Authentication;
public sealed record PersonalTokenIdentity(string OwnerId, string TokenId, bool CanWrite);
