using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using PodcastRssGenerator4DotNet;
using TagLib;
using File = TagLib.File;

namespace DotCast
{
    public class LocalEpisodeProvider : IEpisodeProvider
    {

        private readonly string basePath;
        public LocalEpisodeProvider(string basePath)
        {
            this.basePath = basePath;
        }

        public Feed GetFeed(string podcastName)
        {
            var podcastPath = Path.Combine(basePath, podcastName);
            var feed = new Feed();
            IList<Tuple<string, TagLib.File>> files = new List<Tuple<string, File>>();
            foreach (var file in Directory.GetFiles(podcastPath))
            {
                var metadata = TagLib.File.Create(file);
                if (metadata.Properties.MediaTypes == MediaTypes.Audio || metadata.Properties.MediaTypes == MediaTypes.Video)
                {
                    files.Add(new Tuple<string, File>(file, metadata));
                }
            }

            if (files.All(t => t.Item2.Tag.TrackCount != 0))
            {
                files = files.OrderBy(t => t.Item2.Tag.Track).ToList();
            }
            else if (files.All(t => !t.Item2.Tag.TitleSort.IsNullOrEmpty()))
            {
                files = files.OrderBy(t => t.Item2.Tag.TitleSort).ToList();
            }

            bool feedInfoSet = false;
            foreach (var fileInfo in files)
            {
                if (!feedInfoSet)
                {
                    feed.Title = fileInfo.Item2.Tag.Album;
                    feed.AuthorName = string.Join(", ", fileInfo.Item2.Tag.AlbumArtists);
                    feedInfoSet = true;
                }
                var info = new FileInfo(fileInfo.Item1);
                var episode = new Episode()
                {
                    Duration = fileInfo.Item2.Properties.Duration.ToString("g"),
                    FileLength = (int)info.Length,
                    FileType = info.Extension,
                    FileUrl = GetFileUrl(fileInfo.Item1),
                    Keywords = fileInfo.Item2.Properties.Description,
                    Title = fileInfo.Item2.Tag.Title,
                    SubTitle = fileInfo.Item2.Tag.Subtitle,
                };
                feed.Episodes.Add(episode);
            }

            return feed;
        }

        private string GetFileUrl(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}