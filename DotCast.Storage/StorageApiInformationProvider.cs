using DotCast.Infrastructure.Gateway.Abstractions;
using Microsoft.Extensions.Options;

namespace DotCast.Storage
{
    public class StorageApiInformationProvider(IOptions<GatewayOptions> options) : IStorageApiInformationProvider
    {
        public string GetFileUrl(string audioBookId, string fileName, bool isArchive)
        {
            var baseUrl = options.Value.ApiBaseUrl;
            var baseUri = new Uri(baseUrl);
            Uri fullUri;
            if (!isArchive)
            {
                fullUri = new Uri(baseUri, $"storage/{audioBookId}/{fileName}");
            }
            else
            {
                fullUri = new Uri(baseUri, $"storage/archive/{audioBookId}");
            }

            return fullUri.ToString();
        }
    }
}