using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.BookInfoProvider
{
    public class AudiobookInfoSuggestionsRequestedHandler(IBookInfoProvider bookInfoProvider) : MessageHandler<AudiobookInfoSuggestionsRequest, IReadOnlyCollection<FoundBookInfo>>
    {
        public override async Task<IReadOnlyCollection<FoundBookInfo>> Handle(AudiobookInfoSuggestionsRequest message)
        {
            var result = new List<FoundBookInfo>();
            await foreach (var info in bookInfoProvider.GetBookInfoAsync(message.Name))
            {
                if (message.Count == null && result.Count < message.Count)
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
    }
}