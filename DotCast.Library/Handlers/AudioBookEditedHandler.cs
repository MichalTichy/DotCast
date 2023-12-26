using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class AudioBookEditedHandler(IRepository<AudioBook> repository) : IMessageHandler<AudioBookEdited>
    {
        public async Task Handle(AudioBookEdited message)
        {
            await repository.UpdateAsync(message.AudioBook);
        }
    }
}