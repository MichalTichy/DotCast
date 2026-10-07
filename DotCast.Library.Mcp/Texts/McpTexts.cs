using System.Globalization;
using System.Resources;
namespace DotCast.Library.Mcp.Texts;
public static class McpTexts
{
    private static readonly ResourceManager Resources = new("DotCast.Library.Mcp.Texts.McpTexts", typeof(McpTexts).Assembly);
    public static string Get(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
