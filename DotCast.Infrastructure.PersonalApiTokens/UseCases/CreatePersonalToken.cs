namespace DotCast.Infrastructure.PersonalApiTokens.UseCases;
public sealed record CreatePersonalToken(string Name, DateTimeOffset ExpiresAt, bool CanWrite);
