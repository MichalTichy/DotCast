using Ardalis.GuardClauses;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using Wolverine;

namespace DotCast.Storage.Handlers
{
    public class AudioBookEditedHandler(IStorage storage, IMessageBus messageBus) : IMessageHandler<AudioBookEdited>
    {
        public async Task Handle(AudioBookEdited message)
        {
            await storage.UpdateMetadataAsync(message.AudioBook.AudioBookInfo);

            var updatedEntry = storage.GetStorageEntry(message.AudioBook.Id);
            Guard.Against.Null(updatedEntry, nameof(message.AudioBook.Id));
            var modifiedFiles = updatedEntry.Files.Select(t => t.LocalPath).ToArray();

            await messageBus.SendAsync(new AudioBookReadyForProcessing(message.AudioBook.Id, modifiedFiles));
        }
    }
}