using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.BookInfoProvider
{
    public class AudiobookInfoSuggestionsRequestHandler(IBookInfoProvider bookInfoProvider) : IMessageHandler<AudiobookInfoSuggestionsRequest, IReadOnlyCollection<FoundBookInfo>>
    {
        public async Task<IReadOnlyCollection<FoundBookInfo>> Handle(AudiobookInfoSuggestionsRequest message)
        {
            var result = new List<FoundBookInfo>();
            await foreach (var info in bookInfoProvider.GetBookInfoAsync(message.Name, message.AuthorName))
            {
                if (!IsValidSuggestion(info))
                {
                    continue;
                }

                if (message.Count == null || result.Count < message.Count)
                {
                    result.Add(info);
                }
                else
                {
                    break;
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
