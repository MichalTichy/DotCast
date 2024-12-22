using DotCast.RssGenerator.Base;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.RSS
{
    public class AudioBookRssGenerator : RssGenerator<AudioBook>
    {
        public override Task<Feed> BuildFeed(AudioBook param)
        {
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
                        FileUrl = chapter.Url,
                        PublicationDate = (param.AudioBookInfo.ReleaseDate ?? new DateTime(2000, 1, 1))
                            .AddMinutes(param.AudioBookInfo.Categories.Count)
                            .AddMinutes(-index - 1), //this ensures that episodes are in correct order when ordering is by date (latest first)
                        FileLength = chapter.Size ?? 0,
                        FileType = chapter.FileType
                    })
                    .ToList()
            };
            return Task.FromResult(feed);
        }
    }
}