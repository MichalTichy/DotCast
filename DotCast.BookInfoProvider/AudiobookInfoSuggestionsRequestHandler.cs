using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.BookInfoProvider
{
    public class AudiobookInfoSuggestionsRequestHandler(IEnumerable<IBookInfoProvider> bookInfoProviders)
    {
        public async Task<IReadOnlyCollection<FoundBookInfo>> Handle(AudiobookInfoSuggestionsRequest message, CancellationToken cancellationToken = default)
        {
            var result = new List<FoundBookInfo>();
            foreach (var bookInfoProvider in bookInfoProviders)
            {
                await foreach (var info in bookInfoProvider.GetBookInfoAsync(message.Name, message.AuthorName, cancellationToken))
                {
                    if (!IsValidSuggestion(info))
                    {
                        continue;
                    }

                    if (message.Count == null || result.Count < message.Count)
                    {
                        result.Add(info);
                        if (result.Count >= message.Count) return result;
                    }
                    else
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        private static bool IsValidSuggestion(FoundBookInfo info)
        {
            return !string.IsNullOrWhiteSpace(info.Title)
                   && !string.Equals(info.Title, "ERROR", StringComparison.OrdinalIgnoreCase)
                   && !string.IsNullOrWhiteSpace(info.Author)
                   && !string.Equals(info.Author, "ERROR", StringComparison.OrdinalIgnoreCase);
        }
    }
}
