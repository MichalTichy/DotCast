﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using PodcastRssGenerator4DotNet;
using TagLib;
using File = TagLib.File;

namespace DotCast
{
    public class LocalPodcastProvider : IPodcastProvider
    {

        private readonly string basePath;
        private string baseUrl;

        public LocalPodcastProvider(string basePath, string baseUrl)
        {
            this.basePath = basePath;
            this.baseUrl = baseUrl;
        }

        public Feed GetFeed(string podcastName)
        {
            var podcastPath = Path.Combine(basePath, podcastName);
            if (!Directory.Exists(podcastPath))
                return null;

            var feed = new Feed();
            string image = null;
            IList<(string path, File metadata)> files = new List<(string path, File file)>();
            foreach (var file in Directory.GetFiles(podcastPath))
            {
                File metadata;
                try
                {
                    metadata = File.Create(file);
                }
                catch (UnsupportedFormatException)
                {
                    continue;
                }
                if (metadata.Properties.MediaTypes == MediaTypes.Audio || metadata.Properties.MediaTypes == MediaTypes.Video)
                {
                    files.Add((file, metadata));
                }

                if (metadata.Properties.MediaTypes == MediaTypes.Photo)
                {
                    image = file;
                }
            }

            if (files.All(t => t.metadata.Tag.TrackCount != 0))
            {
                files = files.OrderBy(t => t.metadata.Tag.Track).ToList();
            }
            else if (files.All(t => !t.metadata.Tag.TitleSort.IsNullOrEmpty()))
            {
                files = files.OrderBy(t => t.metadata.Tag.TitleSort).ToList();
            }

            bool feedInfoSet = false;
            for (var index = 0; index < files.Count; index++)
            {
                var fileInfo = files[index];
                if (!feedInfoSet)
                {
                    feed.Title = fileInfo.metadata.Tag.Album ?? podcastName;
                    feed.AuthorName = string.Join(", ", fileInfo.metadata.Tag.AlbumArtists.Union(fileInfo.metadata.Tag.Artists));
                    feed.ImageUrl = GetFileUrl(podcastName, Path.GetFileName(image));
                    feed.Description = fileInfo.metadata.Tag.Description ?? fileInfo.metadata.Tag.Comment;
                    feedInfoSet = true;
                }

                var info = new FileInfo(fileInfo.path);
                var episode = new Episode()
                {
                    Duration = fileInfo.metadata.Properties.Duration.ToString("g"),
                    FileLength = (int)info.Length,
                    FileType = info.Extension,

                    FileUrl = GetFileUrl(podcastName, Path.GetFileName(fileInfo.path)),
                    Title = fileInfo.metadata.Tag.Title ?? Path.GetFileNameWithoutExtension(fileInfo.path),
                    SubTitle = fileInfo.metadata.Tag.Subtitle,
                    PublicationDate = new DateTime((int)(fileInfo.metadata.Tag.Year != default ? fileInfo.metadata.Tag.Year + 1 : 2000), 1, 1).AddMinutes(-index - 1) //this ensures that episodes are in correct order when ordering is by date (latest first)
                };
                feed.Episodes.Add(episode);
            }

            return feed;
        }

        private string GetFileUrl(string podcastName, string fileName)
        {
            if (fileName.IsNullOrEmpty())
            {
                return null;
            }
            return $"{baseUrl}/{AppConfiguration.PodcastsLocation}/{podcastName}/{fileName}";
        }

        public IEnumerable<string> GetPodcastNames()
        {
            var baseDirectory = new DirectoryInfo(basePath);
            foreach (var directory in baseDirectory.GetDirectories())
            {
                yield return directory.Name;
            }
        }
    }
}