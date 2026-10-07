using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;

namespace DotCast.Library.Handlers;

public class SaveAudioBookRequestHandler(IRepository<AudioBook> repository, IStorage storage, IMessagePublisher messenger)
    : IMessageHandler<SaveAudioBookRequest>
{
    public async Task Handle(SaveAudioBookRequest message)
    {
        var draft = message.AudioBook;
        var existing = await repository.GetByIdAsync(draft.Id) ?? throw new UnauthorizedAccessException();
        if (string.IsNullOrWhiteSpace(draft.AudioBookInfo.Name) || draft.Rating is < 0 or > 100 || draft.AudioBookInfo.OrderInSeries < 0)
            throw new ArgumentException("Invalid audiobook metadata.");
        var entry = storage.GetStorageEntry(draft.Id) ?? throw new InvalidOperationException("Audiobook files are unavailable.");
        // File validation/writes must succeed before publishing the changed library record.
        await storage.UpdateMetadataAsync(draft.AudioBookInfo);
        draft.LibraryId = existing.LibraryId;
        draft.AddedAtUtc = existing.AddedAtUtc;
        await repository.UpdateAsync(draft);
        await messenger.PublishAsync(new AudioBookReadyForProcessing(draft.Id, entry.Files.Select(file => file.LocalPath).ToArray()));
    }
}
