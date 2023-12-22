using DotCast.SharedKernel.Messages;
using DotCast.Storage.Storage;

namespace DotCast.Storage.Handlers
{
    public class RestoreFromFileSystemRequestHandler(IStorage storage) : CascadingMessageHandler<RestoreFromFileSystemRequest>
    {
        public override async IAsyncEnumerable<object> Handle(RestoreFromFileSystemRequest message)
        {
            foreach (var storageEntry in storage.GetEntriesAsync())
            {
                var metadata = await storage.ExtractMetadataAsync(storageEntry.Id);
                yield return new NewAudioBookRequest(storageEntry.Id, metadata);
            }
        }
    }
}