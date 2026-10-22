using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Library.Featured;
using DotCast.Library.Playback;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.Extensions.Options;

namespace DotCast.Library.Handlers;

public class AudioBookLibraryRequestHandler(
    IReadOnlyRepository<AudioBook> audioBookRepository,
    IAudioBookUserContextProvider userContextProvider,
    IOptions<FinishedListeningOptions> options)
    : IMessageHandler<AudioBookLibraryRequest, AudioBookLibrary>
{
    public async Task<AudioBookLibrary> Handle(AudioBookLibraryRequest message)
    {
        var context = await userContextProvider.GetForCurrentUserAsync();
        var library = await audioBookRepository.ListAsync(new AudioBookRetrievalSpecification(AudioBookLibraryFilter.Empty));
        var books = new AudioBookRetrievalSpecification(message.Filter ?? AudioBookLibraryFilter.Empty, context).Apply(library);

        // Include every accessible book so an open detail drawer remains current even
        // when a rating or listening change removes its book from the filtered list.
        return new AudioBookLibrary(
            books,
            library.ToDictionary(book => book.Id, context.GetState),
            FeaturedAudioBookSelector.Select(library, books, context, DateTime.UtcNow, options.Value));
    }
}
