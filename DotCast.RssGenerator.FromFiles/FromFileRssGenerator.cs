using DotCast.RssGenerator.Base;
using TagLib;
using File = TagLib.File;

#pragma warning disable CS0618

namespace DotCast.RssGenerator.FromFiles
{
    public class FromFileRssGenerator : RssGenerator<RssFromFileParams>
    {
        public override Task<Feed> BuildFeed(RssFromFileParams param)
        {
            var feed = new Feed(param.PodcastName);
            string? image = null;
            var imageIsDesignatedCover = false;
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;

            IList<(LocalFileInfo info, File metadata)> files = new List<(LocalFileInfo info, File file)>();
            foreach (var file in param.Files)
            {
                File metadata;
                try
                {
                    metadata = File.Create(file.LocalPath);
                }
                catch (KeyNotFoundException)
                {
                    continue;
                }
                catch (UnsupportedFormatException)
                {
                    continue;
                }
                catch (CorruptFileException)
                {
                    continue;
                }

                if (metadata.Properties.MediaTypes == MediaTypes.Audio || metadata.Properties.MediaTypes == MediaTypes.Video)
                {
                    files.Add((file, metadata));
                }

                if (metadata.Properties.MediaTypes == MediaTypes.Photo)
                {
                    if (imageIsDesignatedCover)
                    {
                        continue;
                    }

                    image = file.RemotePath;
                    imageIsDesignatedCover = Path.GetFileNameWithoutExtension(metadata.Name).ToLower() == "cover";
                }
            }

            if (files.All(t => t.metadata.Tag.TrackCount != 0))
            {
                files = files.OrderBy(t => t.metadata.Tag.Track).ToList();
            }
            else if (files.All(t => !string.IsNullOrWhiteSpace(t.metadata.Tag.TitleSort)))
            {
                files = files.OrderBy(t => t.metadata.Tag.TitleSort).ToList();
            }
            else
            {
                files = files.OrderBy(t => t.info.LocalPath).ToList();
            }

            var feedInfoSet = false;
            for (var index = 0; index < files.Count; index++)
            {
                var fileInfo = files[index];
                if (!feedInfoSet)
                {
                    feed.Title = fileInfo.metadata.Tag.Album ?? feed.Title;
                    if (!string.IsNullOrWhiteSpace(fileInfo.metadata.Tag.FirstPerformer))
                    {
                        feed.AuthorName = fileInfo.metadata.Tag.FirstArtist;
                    }
                    else if (!string.IsNullOrWhiteSpace(fileInfo.metadata.Tag.FirstArtist))
                    {
                        feed.AuthorName = fileInfo.metadata.Tag.FirstArtist;
                    }
                    else if (fileInfo.metadata.Tag.Artists != null)
                    {
                        feed.AuthorName = string.Join(", ",
                            fileInfo.metadata.Tag.AlbumArtists.Union(fileInfo.metadata.Tag.Artists));
                    }

                    feed.AuthorName = textInfo.ToTitleCase(feed.AuthorName?.ToLower() ?? string.Empty);
                    feed.ImageUrl = image;
                    feed.Description = fileInfo.metadata.Tag.Description ?? fileInfo.metadata.Tag.Comment;

                    feedInfoSet = true;
                }

                var info = new FileInfo(fileInfo.info.LocalPath);
                var episode = new Episode
                {
                    Duration = fileInfo.metadata.Properties.Duration,
                    FileLength = (int) info.Length,
                    FileType = info.Extension,

                    FileUrl = fileInfo.info.RemotePath,
                    Title = fileInfo.metadata.Tag.Title ?? Path.GetFileNameWithoutExtension(fileInfo.info.RemotePath).Replace('_', ' '),
                    SubTitle = fileInfo.metadata.Tag.Subtitle,
                    PublicationDate = new DateTime((int) (fileInfo.metadata.Tag.Year != default ? fileInfo.metadata.Tag.Year + 1 : 2000), 1, 1)
                        .AddMinutes(-index - 1) //this ensures that episodes are in correct order when ordering is by date (latest first)
                };
                feed.Episodes.Add(episode);
            }

            return Task.FromResult(feed);
        }
    }
}