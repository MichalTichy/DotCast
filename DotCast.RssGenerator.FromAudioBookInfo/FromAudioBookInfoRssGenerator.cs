using DotCast.AudioBookInfo;
using DotCast.RssGenerator.Base;
using System.IO;
using System;
using System.Reflection;

namespace DotCast.RssGenerator.FromAudioBookInfo
{
    public class FromAudioBookInfoRssGenerator : RssGenerator<AudioBook>
    {
        public override Task<Feed> BuildFeed(AudioBook param)
        {
            var feed = new Feed(param.Name)
            {
                AuthorName = param.AuthorName,
                Categories = param.Categories.Select(c => c.Name).ToList(),
                Description = param.Description,
                ImageUrl = param.ImageUrl,
                Episodes = param.Chapters.Select((chapter, index) => new Episode
                    {
                        Title = chapter.Name,
                        Duration = chapter.Duration,
                        FileUrl = chapter.Url,
                        PublicationDate = new DateTime(2000, 1, 1).AddMinutes(-index - 1) //this ensures that episodes are in correct order when ordering is by date (latest first)
                    })
                    .ToList()
            };
            return Task.FromResult(feed);
        }
    }
}