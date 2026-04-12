using Microsoft.Extensions.Options;

namespace DotCast.Infrastructure.UrlBuilder;

public class UrlBuilder(IOptions<UrlBuilderOptions> options) : IUrlBuilder
{
    public string GetAbsoluteUrl(string relativeUrl)
    {
        var baseUri = new Uri(options.Value.BaseUrl);
        return new Uri(baseUri, relativeUrl).AbsoluteUri;
    }
}
