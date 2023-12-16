using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class AudioBookDetailRequestHandler(IReadOnlyRepository<AudioBook> repository) : MessageHandler<AudioBookDetailRequest, AudioBook?>
    {
        public override async Task<AudioBook?> Handle(AudioBookDetailRequest message)
        {
            return await repository.GetByIdAsync(message.Id);
        }
    }
}
