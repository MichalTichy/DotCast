using DotCast.SharedKernel.Messages;
using DotCast.Storage.Storage;

namespace DotCast.Storage.Handlers
{
    public class AudioBookMetadataUpdatedHandler(IStorage storage) : MessageHandler<AudioBookMetadataUpdated>
    {
        public override async Task Handle(AudioBookMetadataUpdated message)
        {
            await storage.UpdateMetadataAsync(message.AudioBook);
        }
    }
}