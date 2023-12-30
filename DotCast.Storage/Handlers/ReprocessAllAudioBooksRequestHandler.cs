using Ardalis.GuardClauses;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;

namespace DotCast.Storage.Handlers
{
    public class ReprocessAllAudioBooksRequestHandler(IStorage storage) : ICascadingMessageHandler<ReprocessAllAudioBooksRequest>
    {
        public IEnumerable<object> Handle(ReprocessAllAudioBooksRequest message)
        {
            foreach (var storageEntry in storage.GetEntriesAsync())
            {
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