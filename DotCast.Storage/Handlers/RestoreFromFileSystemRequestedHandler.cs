using Ardalis.GuardClauses;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Storage;
using Wolverine.Runtime;

namespace DotCast.Storage.Handlers
{
    public class RestoreFromFileSystemRequestHandler(IStorage storage) : IAsyncCascadingMessageHandler<RestoreFromFileSystemRequest>
    {
        public async IAsyncEnumerable<object> Handle(RestoreFromFileSystemRequest message)
        {
            foreach (var storageEntry in storage.GetEntries())
            {
                var extendedEntry = storage.GetStorageEntry(storageEntry.Id);
                Guard.Against.Null(extendedEntry, nameof(extendedEntry));
                var metadata = await storage.ExtractMetadataAsync(storageEntry.Id);
                yield return new AudioBookStorageMetadataUpdated(metadata);
            }
        }
    }
}