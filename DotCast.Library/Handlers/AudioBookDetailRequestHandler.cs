using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Shared.Infrastructure.Persistence.Repositories;

namespace DotCast.Library.Handlers
{
    public class AudioBookDetailRequestHandler(IReadOnlyRepository<AudioBook> repository) : IMessageHandler<AudioBookDetailRequest, AudioBook?>
    {
        public async Task<AudioBook?> Handle(AudioBookDetailRequest message)
        {
            return await repository.GetByIdAsync(message.Id);
        }
    }
}
