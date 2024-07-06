using DotCast.Infrastructure.M3U;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using TagLib;
using Xabe.FFmpeg;
using File = TagLib.File;

namespace DotCast.Infrastructure.MetadataManager
{
    public class MetadataManager(M3uManager m3UManager, ILogger<MetadataManager> logger) : IMetadataManager
    {
        public async Task<AudioBookInfo> ExtractMetadata(StorageEntryWithFiles source, CancellationToken cancellationToken = default)
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
            List<string>? fileOrder = null;
            var chapters = new List<Chapter>(files.Count);
            Category[]? categories = null;
            try
            {
                foreach (var file in source.Files)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return await Task.FromCanceled<AudioBookInfo>(cancellationToken);
                    }

                    if (file.LocalPath.EndsWith(".m3u"))
                    {
                        fileOrder = (await m3UManager.ReadM3uFile(file.LocalPath)).ToList();
                        continue;
                    }

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
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to read metadata for file {0}", file);
                        continue;
                    }

                    if (metadata.Properties.MediaTypes == MediaTypes.Audio || metadata.Properties.MediaTypes == MediaTypes.Video)
                    {
                        files.Add((file, metadata));
                    }
                    else if (metadata.Properties.MediaTypes == MediaTypes.Photo)
                    {
                        if (imageIsDesignatedCover)
                        {
                            continue;
                        }

                        image = file.RemotePath;
                        imageIsDesignatedCover = Path.GetFileNameWithoutExtension(metadata.Name).ToLower() == "cover";
                    }
                }

                if (fileOrder != null && files.Select(t => Path.GetFileName(t.info.LocalPath)).All(t => fileOrder.Contains(t)))
                {
                    files = files.OrderBy(file => fileOrder.IndexOf(Path.GetFileName(file.info.LocalPath))).ToList();
                }

                if (files.Select(t => t.metadata.Tag.Track).GroupBy(t => t).All(t => t.Count() == 1))
                {
                    files = files.OrderBy(t => t.metadata.Tag.Track).ToList();
                }
                else
                {
                    files = files.OrderBy(t => t.info.LocalPath).ToList();
                }

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

                    if (categories == null)
                    {
                        if (fileInfo.metadata.Tag.Genres is { Length: > 0 })
                        {
                            var allCategories = Category.GetAll();
                            categories = fileInfo.metadata.Tag.Genres.Select(t => allCategories.FirstOrDefault(c => c.Name == t)).Where(t => t != null).Select(t => t!).ToArray();
                        }
                    }

                    var info = new FileInfo(fileInfo.info.LocalPath);

                    var chapterName = fileInfo.metadata.Tag.Title ?? Path.GetFileNameWithoutExtension(fileInfo.info.RemotePath).Replace('_', ' ');
                    TimeSpan duration;
                    try
                    {
                        duration = await GetAudioDuration(fileInfo.info.LocalPath);
                    }
                    catch (Exception e)
                    {
                        duration = fileInfo.metadata.Properties.Duration;
                        logger.LogWarning(e, "Failed to extract duration from {0}.", fileInfo.info.LocalPath);
                    }

                    var chapter = new Chapter
                    {
                        Name = chapterName,
                        Duration = duration,
                        Url = fileInfo.info.RemotePath,
                        Size = info.Length
                    };
                    chapters.Add(chapter);
                }
            }
            finally
            {
                foreach (var file in files.Select(t => t.metadata))
                {
                    file.Dispose();
                }
            }

            var audioBookName = title ?? source.Id;
            var audioBook = new AudioBookInfo
            {
                Id = source.Id,
                Name = audioBookName,
                AuthorName = author ?? string.Empty,
                Chapters = chapters,
                ReleaseDate = releaseDate,
                Description = description,
                ImageUrl = image,
                SeriesName = series,
                OrderInSeries = orderInSeries ?? 0,
                ArchiveUrl = source.Archive?.RemotePath,
                Categories = categories ?? Array.Empty<Category>()
            };
            return audioBook;
        }

        private async Task<TimeSpan> GetAudioDuration(string source)
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(source);
            return mediaInfo.Duration;
        }

        public async Task UpdateMetadata(AudioBookInfo audioBook, StorageEntryWithFiles source, CancellationToken cancellationToken = default)
        {
            var audioFiles = new List<string>();
            string? m3UFile = null;
            var directory = Path.GetDirectoryName(source.Files.First().LocalPath)!;
            foreach (var localFileInfo in source.Files)
            {
                try
                {
                    if (localFileInfo.LocalPath.EndsWith(".m3u"))
                    {
                        m3UFile = localFileInfo.LocalPath;
                        continue;
                    }

                    using var file = File.Create(localFileInfo.LocalPath);
                    if (file.Properties.MediaTypes != MediaTypes.Audio && file.Properties.MediaTypes != MediaTypes.Video)
                    {
                        continue;
                    }

                    audioFiles.Add(localFileInfo.LocalPath);

                    file.Tag.Album = audioBook.Name;
                    file.Tag.AlbumArtists = Array.Empty<string>();
                    file.Tag.Performers = new[] { audioBook.AuthorName };
                    file.Tag.Grouping = audioBook.SeriesName;
                    file.Tag.TitleSort = audioBook.OrderInSeries.ToString();
                    file.Tag.Genres = audioBook.Categories.Select(t => t.Name).ToArray();
                    file.Tag.Description = audioBook.Description;
                    var matchedChapter = audioBook.Chapters.FirstOrDefault(t => t.Url == localFileInfo.RemotePath);

                    if (matchedChapter != null)
                    {
                        file.Tag.Title = matchedChapter.Name;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    file.Tag.Year = (uint) (audioBook.ReleaseDate?.Year ?? 0);
                    file.Save();
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Failed to update metadata");
                }
            }

            var m3UFileDestination = m3UFile ?? Path.Combine(directory, "index.m3u");
            await m3UManager.GenerateM3uFile(audioFiles.Select(Path.GetFileName)!, m3UFileDestination);
        }
    }
}