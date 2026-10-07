using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers;

public class AudioBookNameAvailabilityRequestHandler(IReadOnlyRepository<AudioBook> repository)
    : IMessageHandler<AudioBookNameAvailabilityRequest, AudioBookNameAvailability>
{
    public async Task<AudioBookNameAvailability> Handle(AudioBookNameAvailabilityRequest message)
    {
        var id = AudioBookId.FromName(message.Name);
        var exists = await repository.GetBySpecAsync(new AudioBookExistenceCheckSpecification(id));
        return new(id, exists);
    }
}
