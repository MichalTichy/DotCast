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
            var audioBook = await repository.GetByIdAsync(message.AudioBookInfo.Id);
            if (audioBook == null)
            {
                await repository.AddAsync(new AudioBook
                {
                    Id = message.AudioBookInfo.Id,
                    AudioBookInfo = message.AudioBookInfo
                });
            }
            else
            {
                audioBook.AudioBookInfo = message.AudioBookInfo;
                await repository.UpdateAsync(audioBook);
            }
        }
    }
}