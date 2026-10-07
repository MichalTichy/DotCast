using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using DotCast.Infrastructure.CurrentUserProvider;

namespace DotCast.Storage.Handlers
{
    public class RestoreFromFileSystemRequestHandler(IStorage storage, ICurrentUserProvider<UserInfo> currentUserProvider, IMessagePublisher messenger, ILogger<RestoreFromFileSystemRequestHandler> logger)
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
                yield return new ProcessingJobChanged(storageEntry.Id, null, DateTime.UtcNow);
                var extendedEntry = storage.GetStorageEntry(storageEntry.Id);
                if (extendedEntry == null)
                {
                    logger.LogError($"Failed to get storage info for {storageEntry.Id}");
                    yield return new ProcessingJobChanged(storageEntry.Id, false, DateTime.UtcNow);
                    continue;
                }

                var succeeded = false;
                try
                {
                    var metadata = await storage.ExtractMetadataAsync(storageEntry.Id);
                    await messenger.ExecuteAsync(new AudioBookStorageMetadataUpdated(metadata));
                    succeeded = true;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Restore failed for {AudioBookId}", storageEntry.Id);
                }
                yield return new ProcessingJobChanged(storageEntry.Id, succeeded, DateTime.UtcNow);
            }
        }
    }
}
