using DotCast.Infrastructure.PresignedUrls;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.UrlBuilder;

namespace DotCast.Library
{
    public class LibraryApiInformationProvider(IUrlBuilder urlBuilder, ICurrentUserIdProvider userIdProvider, IPresignedUrlManager presignedUrlManager) : ILibraryApiInformationProvider
    {
        public async Task<string> GetFeedUrlAsync(string audioBookId)
        {
            var userId = await userIdProvider.GetCurrentUserIdRequiredAsync();
            var fullUri = urlBuilder.GetAbsoluteUrl($"library/{audioBookId}/{userId}/rss");
            var signedUrl = presignedUrlManager.GenerateUrl(fullUri);
            return signedUrl;
        }
    }
}
