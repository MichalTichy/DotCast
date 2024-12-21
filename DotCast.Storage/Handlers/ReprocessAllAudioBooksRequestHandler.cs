using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.AppUser.Identity;
using Shared.Infrastructure.CurrentUserProvider;

namespace DotCast.Storage.Handlers
{
    public class ReprocessAllAudioBooksRequestHandler(IStorage storage, ICurrentUserProvider<UserInfo> currentUserProvider, ILogger<ReprocessAllAudioBooksRequestHandler> logger)
        : IAsyncCascadingMessageHandler<ReprocessAllAudioBooksRequest>
    {
        public async IAsyncEnumerable<object> Handle(ReprocessAllAudioBooksRequest message)
        {
            var user = await currentUserProvider.GetCurrentUserRequiredAsync();
            if (!user.IsAdmin)
            {
                throw new NotSupportedException("Only admins can do this action.");
            }

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