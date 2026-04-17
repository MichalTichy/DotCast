using DotCast.Infrastructure.PresignedUrls;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.UrlBuilder;
using DotCast.SharedKernel.Messages;
using DotCast.Storage;

namespace DotCast.Library
{
    public class LibraryApiInformationProvider(
        IUrlBuilder urlBuilder,
        ICurrentUserIdProvider userIdProvider,
        IPresignedUrlManager presignedUrlManager,
        IStorageApiInformationProvider storageApiInformationProvider,
        IMessagePublisher messenger) : ILibraryApiInformationProvider
    {
        public async Task<string> GetFeedUrlAsync(string audioBookId)
        {
            var userId = await userIdProvider.GetCurrentUserIdRequiredAsync();
            var fullUri = urlBuilder.GetAbsoluteUrl($"library/{audioBookId}/{userId}/rss");
            var signedUrl = presignedUrlManager.GenerateUrl(fullUri);
            await messenger.PublishAsync(new AudioBookRssLinkGenerated(audioBookId));
            return signedUrl;
        }

        public async Task<string> GetArchiveUrlAsync(string audioBookId)
        {
            var userId = await userIdProvider.GetCurrentUserIdRequiredAsync();
            return storageApiInformationProvider.GetFileUrl(audioBookId, $"{audioBookId}.zip", true, userId: userId);
        }
    }
}
