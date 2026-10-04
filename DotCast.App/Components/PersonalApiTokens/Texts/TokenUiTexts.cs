using System.Globalization;
using System.Resources;
namespace DotCast.App.Components.PersonalApiTokens.Texts;
public static class TokenUiTexts
{
    private static readonly ResourceManager Resources = new("DotCast.App.Components.PersonalApiTokens.Texts.TokenUiTexts", typeof(TokenUiTexts).Assembly);
    public static string Get(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
