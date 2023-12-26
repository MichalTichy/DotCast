using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Library.RSS;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

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

            return await rssGenerator.GenerateRss(audioBook);
        }
    }
}