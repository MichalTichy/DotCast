using System.Globalization;
using System.Resources;
namespace DotCast.App.Components.ApiKey.Texts;
public static class ApiKeyTexts
{
    private static readonly ResourceManager Resources = new("DotCast.App.Components.ApiKey.Texts.ApiKeyTexts", typeof(ApiKeyTexts).Assembly);
    public static string Get(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
