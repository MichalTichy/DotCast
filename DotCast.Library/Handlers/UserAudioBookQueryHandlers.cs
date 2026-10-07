using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Library.Featured;
using DotCast.Library.Playback;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.Extensions.Options;

namespace DotCast.Library.Handlers
{
    public class AudioBookUserStateRequestHandler(
        IReadOnlyRepository<AudioBook> audioBookRepository,
        IAudioBookUserContextProvider userContextProvider)
        : IMessageHandler<AudioBookUserStateRequest, IReadOnlyDictionary<string, AudioBookUserState>>
    {
        public async Task<IReadOnlyDictionary<string, AudioBookUserState>> Handle(AudioBookUserStateRequest message)
        {
            if (message.AudioBookIds.Count == 0)
            {
                return new Dictionary<string, AudioBookUserState>();
            }

            var context = await userContextProvider.GetForCurrentUserAsync();
            var requestedIds = message.AudioBookIds.ToHashSet();
            var audioBooks = await audioBookRepository.ListAsync();

            return audioBooks
                .Where(book => requestedIds.Contains(book.Id))
                .ToDictionary(book => book.Id, context.GetState);
        }
    }

    public class BooksToRateRequestHandler(
        IReadOnlyRepository<AudioBook> audioBookRepository,
        IAudioBookUserContextProvider userContextProvider,
        IOptions<FinishedListeningOptions> options)
        : IMessageHandler<BooksToRateRequest, IReadOnlyList<BookToRateItem>>
    {
        public async Task<IReadOnlyList<BookToRateItem>> Handle(BooksToRateRequest message)
        {
            var context = await userContextProvider.GetForCurrentUserAsync();
            var audioBooks = (await audioBookRepository.ListAsync()).ToDictionary(book => book.Id);
            var now = DateTime.UtcNow;

            var result = new List<BookToRateItem>();
            foreach (var playback in context.Playbacks.Values)
            {
                if (context.GetRating(playback.AudioBookId).HasValue ||
                    !audioBooks.TryGetValue(playback.AudioBookId, out var audioBook))
                {
                    continue;
                }

                var evaluation = FinishedListeningEstimator.Evaluate(playback, now, options.Value);
                if (!evaluation.IsLikelyFinished)
                {
                    continue;
                }

                result.Add(new BookToRateItem(
                    audioBook.Id,
                    audioBook.AudioBookInfo.Name,
                    audioBook.AudioBookInfo.Description,
                    audioBook.AudioBookInfo.ImageUrl,
                    playback.LastActivityAt));
            }

            return result
                .OrderByDescending(item => item.LastActivityAt)
                .ToList();
        }
    }

    public class FeaturedAudioBookRequestHandler(
        IReadOnlyRepository<AudioBook> audioBookRepository,
        IAudioBookUserContextProvider userContextProvider,
        IOptions<FinishedListeningOptions> options)
        : IMessageHandler<FeaturedAudioBookRequest, FeaturedAudioBook?>
    {
        public async Task<FeaturedAudioBook?> Handle(FeaturedAudioBookRequest message)
        {
            var context = await userContextProvider.GetForCurrentUserAsync();
            var library = await audioBookRepository.ListAsync();
            var filter = message.Filter ?? AudioBookLibraryFilter.Empty;
            var candidates = filter.HasActiveFilters
                ? new AudioBookRetrievalSpecification(filter, context).Apply(library)
                : library;

            return FeaturedAudioBookSelector.Select(library, candidates, context, DateTime.UtcNow, options.Value);
        }
    }
}
