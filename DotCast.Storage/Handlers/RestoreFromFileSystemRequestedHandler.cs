using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Logging;

namespace DotCast.Storage.Handlers
{
    public class RestoreFromFileSystemRequestHandler(IStorage storage, ILogger<RestoreFromFileSystemRequestHandler> logger) : IAsyncCascadingMessageHandler<RestoreFromFileSystemRequest>
    {
        public async IAsyncEnumerable<object> Handle(RestoreFromFileSystemRequest message)
        {
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