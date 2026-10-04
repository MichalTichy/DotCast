using System.Net;
namespace DotCast.Infrastructure.BookInfoProvider.Base;
public static class BookInfoHttpClient
{
    public static HttpClient Create(string language)
    {
        var client = new HttpClient(new SocketsHttpHandler {
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 3,
            ConnectTimeout = TimeSpan.FromSeconds(3)
        }) { Timeout = TimeSpan.FromSeconds(5) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(language);
        return client;
    }
}
