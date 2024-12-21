using DotCast.Infrastructure.PresignedUrls;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.UrlBuilder;

namespace DotCast.Storage
{
    public class StorageApiInformationProvider(IUrlBuilder urlBuilder, IPresignedUrlManager presignedUrlManager) : IStorageApiInformationProvider
    {
        public string GetFileUrl(string audioBookId, string fileName, bool isArchive, bool limitValidity)
        {
            var fullUri = urlBuilder.GetAbsoluteUrl(!isArchive ? $"storage/file/{audioBookId}/{fileName}" : $"storage/archive/{audioBookId}");
            fullUri = presignedUrlManager.GenerateUrl(fullUri, limitValidity ? TimeSpan.FromHours(3) : null);
            return fullUri;
        }
    }
}