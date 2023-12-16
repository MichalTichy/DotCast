using Ardalis.GuardClauses;
using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class AudioBookMetadataUpdatedHandler(IRepository<AudioBook> repository) : MessageHandler<AudioBookMetadataUpdated>
    {
        public override async Task Handle(AudioBookMetadataUpdated message)
        {
            await repository.UpdateAsync(message.AudioBook);
        }
    }
}