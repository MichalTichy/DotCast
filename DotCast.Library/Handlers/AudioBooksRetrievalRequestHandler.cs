using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class AudioBooksRetrievalRequestHandler(IReadOnlyRepository<AudioBook> repository) : MessageHandler<AudioBooksRetrievalRequest, IReadOnlyList<AudioBook>>
    {
        public override async Task<IReadOnlyList<AudioBook>> Handle(AudioBooksRetrievalRequest message)
        {
            var specification = new AudioBookRetrievalSpecification(message.Filter);
            return await repository.ListAsync(specification);
        }
    }
}