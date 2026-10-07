using DotCast.Infrastructure.Messaging.Base;
using DotCast.Library.Playback;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using DotCast.Infrastructure.Persistence.Repositories;

namespace DotCast.Library.Handlers
{
    public class AudioBooksRetrievalRequestHandler(IReadOnlyRepository<AudioBook> repository, IAudioBookUserContextProvider userContextProvider)
        : IMessageHandler<AudioBooksRetrievalRequest, IReadOnlyList<AudioBook>>
    {
        public async Task<IReadOnlyList<AudioBook>> Handle(AudioBooksRetrievalRequest message)
        {
            var filter = message.Filter ?? AudioBookLibraryFilter.Empty;
            var needsUserContext = filter.MinRating.HasValue || filter.MaxRating.HasValue || filter.ListeningState != ListeningStateFilter.Any;
            var userContext = needsUserContext ? await userContextProvider.GetForCurrentUserAsync() : null;

            var specification = new AudioBookRetrievalSpecification(filter, userContext);
            return await repository.ListAsync(specification);
        }
    }
}
