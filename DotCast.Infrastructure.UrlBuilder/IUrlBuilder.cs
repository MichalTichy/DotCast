namespace DotCast.Infrastructure.UrlBuilder;

public interface IUrlBuilder
{
    string GetAbsoluteUrl(string relativeUrl);
}
