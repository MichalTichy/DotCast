using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Shared.Infrastructure.Persistence.Repositories;

namespace DotCast.Library.Handlers
{
    public class AudioBooksRetrievalRequestHandler(IReadOnlyRepository<AudioBook> repository) : IMessageHandler<AudioBooksRetrievalRequest, IReadOnlyList<AudioBook>>
    {
        public async Task<IReadOnlyList<AudioBook>> Handle(AudioBooksRetrievalRequest message)
        {
            var specification = new AudioBookRetrievalSpecification(message.Filter);
            return await repository.ListAsync(specification);
        }
    }
}