using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.RssGenerator.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using DotCast.Storage;

namespace DotCast.Library.RSS
{
    public class AudioBookRssGenerator(
        IStorageApiInformationProvider storageApiInformationProvider,
        IMessagePublisher messenger,
        ICurrentUserIdProvider currentUserIdProvider) : RssGenerator<AudioBook>
    {
        public override async Task<Feed> BuildFeed(AudioBook param)
        {
            var userId = await currentUserIdProvider.GetCurrentUserIdRequiredAsync();
            var feed = new Feed(param.AudioBookInfo.Name)
            {
                AuthorName = param.AudioBookInfo.AuthorName,
                Categories = param.AudioBookInfo.Categories.Select(c => c.Name).ToList(),
                Description = param.AudioBookInfo.Description,
                ImageUrl = param.AudioBookInfo.ImageUrl,
                Episodes = param.AudioBookInfo.Chapters.Select((chapter, index) => new Episode
                    {
                        Title = chapter.Name,
                        Duration = chapter.Duration,
                        FileUrl = BuildChapterUrl(param.Id, chapter.FileId, userId),
                        PublicationDate = (param.AudioBookInfo.ReleaseDate ?? new DateTime(2000, 1, 1))
                            .AddMinutes(param.AudioBookInfo.Categories.Count)
                            .AddMinutes(-index - 1),
                        FileLength = chapter.Size ?? 0,
                        FileType = chapter.FileType
                    })
                    .ToList()
            };

            await messenger.PublishAsync(new AudioBookRssGenerated(param.Id, userId, DateTime.UtcNow));
            return feed;
        }

        private string BuildChapterUrl(string audioBookId, string fileId, string userId)
        {
            return storageApiInformationProvider.GetFileUrl(audioBookId, fileId, false, userId: userId);
        }
    }
}
