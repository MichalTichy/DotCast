using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class AudioBookLibraryFacetsRequestHandler(IReadOnlyRepository<AudioBook> repository)
        : IMessageHandler<AudioBookLibraryFacetsRequest, AudioBookLibraryFacets>
    {
        public async Task<AudioBookLibraryFacets> Handle(AudioBookLibraryFacetsRequest message)
        {
            return await repository.GetRequiredBySpecAsync(new AudioBookLibraryFacetsSpecification());
        }
    }
}
