using System.Globalization;
using System.Resources;
namespace DotCast.Infrastructure.PersonalApiTokens.Texts;
public static class TokenTexts
{
    private static readonly ResourceManager Resources = new("DotCast.Infrastructure.PersonalApiTokens.Texts.TokenTexts", typeof(TokenTexts).Assembly);
    public static string Get(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
