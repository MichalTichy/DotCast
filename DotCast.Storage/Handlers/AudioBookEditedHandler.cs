using System.Runtime.InteropServices;
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
            await storage.UpdateMetadataAsync(message.AudioBook);

            var updatedEntry = storage.GetStorageEntry(message.AudioBook.Id);
            Guard.Against.Null(updatedEntry, nameof(message.AudioBook.Id));
            var modifiedFiles = updatedEntry.Files.Select(t => t.LocalPath).ToArray();

            await messageBus.SendAsync(new FilesModificationsFinished(message.AudioBook.Id, modifiedFiles));
        }
    }
}