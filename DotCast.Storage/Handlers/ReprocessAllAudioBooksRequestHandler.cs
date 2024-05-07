using Ardalis.GuardClauses;
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
                Guard.Against.Null(extendedEntry, nameof(extendedEntry));
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