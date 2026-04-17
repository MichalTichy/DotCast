using DotCast.Infrastructure.PresignedUrls;
using Microsoft.Extensions.Options;
using DotCast.Infrastructure.UrlBuilder;

namespace DotCast.Storage
{
    public class StorageApiInformationProvider(IUrlBuilder urlBuilder, IPresignedUrlManager presignedUrlManager) : IStorageApiInformationProvider
    {
        public string GetFileUrl(string audioBookId, string fileName, bool isArchive, bool limitValidity, string? userId = null)
        {
            var relativeUrl = isArchive
                ? string.IsNullOrWhiteSpace(userId)
                    ? $"storage/archive/{audioBookId}"
                    : $"storage/archive/{audioBookId}/{userId}"
                : string.IsNullOrWhiteSpace(userId)
                    ? $"storage/file/{audioBookId}/{fileName}"
                    : $"storage/file/{audioBookId}/{userId}/{fileName}";

            var fullUri = urlBuilder.GetAbsoluteUrl(relativeUrl);
            fullUri = presignedUrlManager.GenerateUrl(fullUri, limitValidity ? TimeSpan.FromHours(3) : null);
            return fullUri;
        }
    }
}
