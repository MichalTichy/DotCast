using DotCast.Infrastructure.Messaging.Base;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using DotCast.Infrastructure.Persistence.Repositories;

namespace DotCast.Library.Handlers
{
    public class NewAudioBooIdRequestHandler(IRepository<AudioBook> repository) : IMessageHandler<NewAudioBookIdRequest, string>
    {
        public async Task<string> Handle(NewAudioBookIdRequest message)
        {
            var id = AudioBookId.FromName(message.Name);
            var exists = await repository.GetBySpecAsync(new AudioBookExistenceCheckSpecification(id));
            if (exists)
            {
                throw new ArgumentException($"AudioBook with id \"{id}\" already exists");
            }

            return id;
        }

    }
}
