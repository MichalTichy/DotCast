using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.CurrentUserProvider;

namespace DotCast.Storage.Handlers
{
    public class RestoreFromFileSystemRequestHandler(IStorage storage, ICurrentUserProvider<UserInfo> currentUserProvider, ILogger<RestoreFromFileSystemRequestHandler> logger)
        : IAsyncCascadingMessageHandler<RestoreFromFileSystemRequest>
    {
        public async IAsyncEnumerable<object> Handle(RestoreFromFileSystemRequest message)
        {
            var user = await currentUserProvider.GetCurrentUserRequiredAsync();
            if (!user.IsAdmin)
            {
                throw new NotSupportedException("Only admins can do this action.");
            }

            foreach (var storageEntry in storage.GetEntries())
            {
                var extendedEntry = storage.GetStorageEntry(storageEntry.Id);
                if (extendedEntry == null)
                {
                    logger.LogError($"Failed to get storage info for {storageEntry.Id}");
                    continue;
                }

                var metadata = await storage.ExtractMetadataAsync(storageEntry.Id);
                yield return new AudioBookStorageMetadataUpdated(metadata);
            }
        }
    }
}