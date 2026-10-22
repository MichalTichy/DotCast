using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Library.Playback;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.Extensions.Options;

namespace DotCast.Library.Handlers
{
    public class BooksToRateRequestHandler(
        IReadOnlyRepository<AudioBook> audioBookRepository,
        IAudioBookUserContextProvider userContextProvider,
        IOptions<FinishedListeningOptions> options)
        : IMessageHandler<BooksToRateRequest, IReadOnlyList<BookToRateItem>>
    {
        public async Task<IReadOnlyList<BookToRateItem>> Handle(BooksToRateRequest message)
        {
            var context = await userContextProvider.GetForCurrentUserAsync();
            var audioBooks = (await audioBookRepository.ListAsync(new AudioBookRetrievalSpecification(AudioBookLibraryFilter.Empty)))
                .ToDictionary(book => book.Id);
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
}
