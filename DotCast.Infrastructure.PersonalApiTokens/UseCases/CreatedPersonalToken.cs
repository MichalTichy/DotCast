using DotCast.Infrastructure.PersonalApiTokens.Models;
namespace DotCast.Infrastructure.PersonalApiTokens.UseCases;
public sealed record CreatedPersonalToken(string Credential, PersonalTokenInfo Info);
