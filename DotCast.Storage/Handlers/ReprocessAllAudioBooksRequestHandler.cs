using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Logging;

namespace DotCast.Storage.Handlers
{
    public class ReprocessAllAudioBooksRequestHandler(IStorage storage, ILogger<ReprocessAllAudioBooksRequestHandler> logger) : ICascadingMessageHandler<ReprocessAllAudioBooksRequest>
    {
        public IEnumerable<object> Handle(ReprocessAllAudioBooksRequest message)
        {
            var storageEntries = storage.GetEntries().ToArray();
            logger.LogWarning("Reprocessing {Count} audiobooks", storageEntries.Length);
            foreach (var storageEntry in storageEntries)
            {
                logger.LogInformation("Reprocessing {Id}", storageEntry.Id);
                var extendedEntry = storage.GetStorageEntry(storageEntry.Id);
                if (extendedEntry == null)
                {
                    logger.LogError($"Failed to get storage info for {storageEntry.Id}");
                    continue;
                }

                var modifiedFiles = extendedEntry.Files.Select(t => t.LocalPath).ToList();
                if (message.Unzip && extendedEntry.Archive != null)
                {
                    modifiedFiles.Add(extendedEntry.Archive.LocalPath);
                }

                yield return new AudioBookReadyForProcessing(extendedEntry.Id, modifiedFiles);
            }
        }
    }
}