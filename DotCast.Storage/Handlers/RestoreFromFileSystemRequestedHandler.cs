using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Storage;

namespace DotCast.Storage.Handlers
{
    public class RestoreFromFileSystemRequestHandler(IStorage storage) : ICascadingMessageHandler<RestoreFromFileSystemRequest>
    {
        public async IAsyncEnumerable<object> Handle(RestoreFromFileSystemRequest message)
        {
            foreach (var storageEntry in storage.GetEntriesAsync())
            {
                var metadata = await storage.ExtractMetadataAsync(storageEntry.Id);
                yield return new AudioBookStorageMetadataUpdated(metadata);
            }
        }
    }
}