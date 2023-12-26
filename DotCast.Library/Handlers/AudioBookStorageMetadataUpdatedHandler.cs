using Ardalis.GuardClauses;
using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class AudioBookStorageMetadataUpdatedHandler(IRepository<AudioBook> repository) : IMessageHandler<AudioBookStorageMetadataUpdated>
    {
        public async Task Handle(AudioBookStorageMetadataUpdated message)
        {
            await repository.StoreAsync(message.AudioBook);
        }
    }
}