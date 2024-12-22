using DotCast.Infrastructure.Messaging.Base;
using DotCast.Library.RSS;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Shared.Infrastructure.Persistence.Repositories;

namespace DotCast.Library.Handlers
{
    public class AudioBookRssRequestHandler(IReadOnlyRepository<AudioBook> repository, AudioBookRssGenerator rssGenerator) : IMessageHandler<AudioBookRssRequest, string?>
    {
        public async Task<string?> Handle(AudioBookRssRequest message)
        {
            var audioBook = await repository.GetByIdAsync(message.Id);
            if (audioBook == null)
            {
                return null;
            }

            var rss = await rssGenerator.GenerateRss(audioBook);
            return rss;
        }
    }
}