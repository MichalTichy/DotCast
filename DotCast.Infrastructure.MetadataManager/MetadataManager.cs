using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;
using TagLib;
using File = TagLib.File;

namespace DotCast.Infrastructure.MetadataManager
{
    public class MetadataManager : IMetadataManager
    {
        public Task<AudioBook> ExtractMetadata(StorageEntryWithFiles source)
        {
            string? image = null;
            var imageIsDesignatedCover = false;
            string? title = null;
            string? author = null;
            string? description = null;
            string? series = null;
            int? orderInSeries = null;
            DateTime? releaseDate = null;
            List<(LocalFileInfo info, File metadata)> files = new(source.Files.Count);
            foreach (var file in source.Files)
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

            var chapters = new List<Chapter>(files.Count);
            foreach (var fileInfo in files)
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = fileInfo.metadata.Tag.Album;
                }

                if (string.IsNullOrWhiteSpace(author))
                {
                    author = string.Join(" ,", fileInfo.metadata.Tag.AlbumArtists.Union(fileInfo.metadata.Tag.Performers).Distinct());
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    description = fileInfo.metadata.Tag.Description ?? fileInfo.metadata.Tag.Comment;
                }

                if (releaseDate == null)
                {
                    releaseDate = fileInfo.metadata.Tag.Year != 0 ? new DateTime((int) fileInfo.metadata.Tag.Year, 1, 1) : null;
                }

                if (string.IsNullOrWhiteSpace(series))
                {
                    series = fileInfo.metadata.Tag.Grouping;
                }

                if (orderInSeries == null)
                {
                    if (fileInfo.metadata.Tag.TitleSort != null)
                    {
                        if (int.TryParse(fileInfo.metadata.Tag.TitleSort, out var order))
                        {
                            orderInSeries = order;
                        }
                    }
                }

                var info = new FileInfo(fileInfo.info.LocalPath);
                var chapter = new Chapter
                {
                    Name = fileInfo.metadata.Tag.Title ?? Path.GetFileNameWithoutExtension(fileInfo.info.RemotePath).Replace('_', ' '),
                    Duration = fileInfo.metadata.Properties.Duration,
                    Url = fileInfo.info.RemotePath,
                    Size = info.Length
                };
                chapters.Add(chapter);
            }

            var audioBook = new AudioBook
            {
                Id = source.Id,
                Name = title ?? source.Id,
                AuthorName = author ?? "Unknown author",
                Chapters = chapters,
                ReleaseDate = releaseDate,
                Description = description,
                ImageUrl = image,
                SeriesName = series,
                OrderInSeries = orderInSeries ?? 0
            };
            return Task.FromResult(audioBook);
        }

        public Task UpdateMetadata(AudioBook audioBook, StorageEntryWithFiles source)
        {
            foreach (var localFileInfo in source.Files)
            {
                var file = File.Create(localFileInfo.LocalPath);
                file.Tag.Album = audioBook.Name;
                file.Tag.Performers = new[] { audioBook.AuthorName };

                file.Tag.Grouping = audioBook.SeriesName;
                file.Tag.TitleSort = audioBook.OrderInSeries.ToString();

                file.Tag.Description = audioBook.Description;

                var matchedChapter = audioBook.Chapters.FirstOrDefault(t => t.Url == localFileInfo.RemotePath);

                if (matchedChapter != null)
                {
                    file.Tag.Title = matchedChapter.Name;
                    file.Tag.TitleSort = audioBook.Chapters.IndexOf(matchedChapter).ToString("D5");
                }

                file.Save();
            }

            return Task.CompletedTask;
        }
    }
}