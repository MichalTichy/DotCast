using System.Globalization;
using System.IO.Compression;
using System.Text;
using DotCast.AudioBookInfo;
using DotCast.AudioBookProvider.Base;
using DotCast.RssGenerator.Base;
using DotCast.RssGenerator.FromFiles;
using Microsoft.Extensions.Options;
using File = TagLib.File;

namespace DotCast.AudioBookProvider.FileSystem
{
    public class FileSystemAudioBookProvider : IAudioBookInfoProvider, IAudioBookFeedProvider, IAudioBookUploader, IAudioBookDownloader
    {
        private readonly IOptions<FileSystemAudioBookProviderOptions> options;
        private readonly FromFileRssGenerator rssGenerator;
        private FileSystemAudioBookProviderOptions settings => options.Value;

        public FileSystemAudioBookProvider(IOptions<FileSystemAudioBookProviderOptions> options, FromFileRssGenerator rssGenerator)
        {
            this.options = options;
            this.rssGenerator = rssGenerator;
        }

        public async Task<string> GetRss(string id)
        {
            var rssGeneratorParams = GetFeedParams(id);

            return await rssGenerator.GenerateRss(rssGeneratorParams);
        }

        private RssFromFileParams GetFeedParams(string id)
        {
            var audioBookName = GetNormalizedName(id);

            var audioBookPath = Path.Combine(settings.AudioBooksLocation, id);
            var directory = new DirectoryInfo(audioBookPath);
            var filePaths = directory.GetFiles().Select(t => new LocalFileInfo(t.FullName, GetFileUrl(t.Directory!.Name, t.Name))).ToArray();

            var rssGeneratorParams = new RssFromFileParams(audioBookName, filePaths);
            return rssGeneratorParams;
        }

        public async IAsyncEnumerable<AudioBook> GetAudioBooks(string? searchText = null)
        {
            var baseDirectory = new DirectoryInfo(settings.AudioBooksLocation);
            foreach (var directory in baseDirectory.GetDirectories())
            {
                var audioBook = await Get(directory.Name);
                if (audioBook == null)
                {
                    continue;
                }

                var matchesSearchText = searchText == null ||
                                        audioBook.Name.Contains(searchText) ||
                                        audioBook.AuthorName.Contains(searchText) ||
                                        (audioBook.SeriesName?.Contains(searchText) ?? false);
                if (!matchesSearchText)
                {
                    continue;
                }

                yield return audioBook;
            }
        }

        public Task UpdateAudioBook(AudioBook audioBook)
        {
            var localFileInfos = GetFiles(audioBook.Id, out _);
            foreach (var localFileInfo in localFileInfos)
            {
                try
                {
                    var file = File.Create(localFileInfo.LocalPath);
                    file.Tag.Album = audioBook.Name;
                    file.Tag.Performers = new[] {audioBook.AuthorName};

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
                catch (Exception)
                {
                    // ignored
                }
            }

            return Task.CompletedTask;
        }

        public async Task<AudioBook?> Get(string id)
        {
            var filePaths = GetFiles(id, out var directory);

            var rssGeneratorParams = new RssFromFileParams(id, filePaths);

            var feed = await rssGenerator.BuildFeed(rssGeneratorParams);


            return new AudioBook
            {
                Id = id,
                Name = feed.Title,
                AuthorName = feed.AuthorName ?? "Unknown author",
                Description = feed.Description,
                ImageUrl = feed.ImageUrl,
                OrderInSeries = 0,
                Rating = 0,
                SeriesName = null,
                Url = $"{settings.AudioBookServerUrl}/AudioBook/{directory.Name}",
                Chapters = feed.Episodes.Select(t => new Chapter {Name = t.Title, Url = t.FileUrl, Duration = t.Duration}).ToList()
            };
        }

        public async Task<AudioBooksStatistics> GetStatistics()
        {
            var audioBooks = await GetAudioBooks().ToListAsync();
            return AudioBooksStatistics.Create(audioBooks);
        }

        private ICollection<LocalFileInfo> GetFiles(string id, out DirectoryInfo directory)
        {
            var path = Path.Combine(settings.AudioBooksLocation, id);
            directory = new DirectoryInfo(path);
            var filePaths = directory.GetFiles().Select(t => new LocalFileInfo(t.FullName, GetFileUrl(t.Directory!.Name, t.Name))).ToArray();
            return filePaths;
        }

        public IEnumerable<string> GetAudioBookIdsAvailableForDownload()
        {
            return Directory.GetFiles(options.Value.ZippedAudioBooksLocation).Select(Path.GetFileNameWithoutExtension).Where(t => t != null).Select(t => t!);
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(normalizedString.Length);

            for (var i = 0; i < normalizedString.Length; i++)
            {
                var c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        private static string GetNormalizedName(string directory)
        {
            return directory.Replace("_", " ");
        }

        private static string GetEscapedName(string name)
        {
            var noSpaces = name;
            return RemoveDiacritics(noSpaces);
        }

        private string GetFileUrl(string directoryName, string fileName)
        {
            return $"{settings.AudioBookServerUrl}/files/{directoryName}/{fileName}";
        }

        private string GetZipUrl(string fileName)
        {
            return $"{settings.AudioBookServerUrl}/zip/{fileName}";
        }


        public async Task GenerateZipForDownload(string audioBookId, bool replace = false)
        {
            var path = GetAudioBookZipPath(audioBookId);
            if (!replace && System.IO.File.Exists(path))
            {
                return;
            }

            var audioBookDirectory = GetAudioBookDirectory(audioBookId);
            var files = Directory.GetFiles(audioBookDirectory);

            var tmpSuffix = ".tmp";

            var tmpPath = $"{path}{tmpSuffix}";
            await using (var zipFileStream = GetAudioBookZipWriteStream(tmpPath))
            {
                using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Update);

                foreach (var botFilePath in files)
                {
                    var name = Path.GetFileName(botFilePath);
                    var entry = archive.CreateEntry(name, CompressionLevel.SmallestSize);
                    await using var entryStream = entry.Open();
                    await using var fileStream = System.IO.File.OpenRead(botFilePath);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            System.IO.File.Move(tmpPath, path);
        }


        public bool IsDownloadSupported(string audioBookId)
        {
            return System.IO.File.Exists(GetAudioBookZipPath(audioBookId));
        }

        public Task<string> GetZipDownloadUrl(string audioBookId)
        {
            var audioBookZipPath = GetAudioBookZipPath(audioBookId);

            if (!System.IO.File.Exists(audioBookZipPath))
            {
                throw new ArgumentException("Unable to find requested zip");
            }

            var fileName = Path.GetFileName(audioBookZipPath);
            return Task.FromResult(GetZipUrl(fileName));
        }

        public FileStream GetAudioBookFileWriteStream(string audioBookName, string fileName, string fileContentType, out string audioBookId)
        {
            audioBookId = GetEscapedName(audioBookName);
            var finalDirectoryPath = GetAudioBookDirectory(audioBookId);
            var targetFileName = GetEscapedName(fileName);
            var fileFilePath = Path.Combine(finalDirectoryPath, targetFileName);
            if (!Directory.Exists(finalDirectoryPath))
            {
                Directory.CreateDirectory(finalDirectoryPath);
            }

            return new FileStream(fileFilePath, FileMode.Create);
        }

        public FileStream GetAudioBookZipWriteStream(string audioBookName, out string audioBookId)
        {
            audioBookId = GetEscapedName(audioBookName);

            var path = GetAudioBookZipPath(audioBookId);

            return GetAudioBookZipWriteStream(path);
        }

        protected FileStream GetAudioBookZipWriteStream(string targetPath)
        {
            return GetAudioBookZipStream(targetPath, FileMode.Create);
        }

        protected FileStream GetAudioBookZipStream(string targetPath, FileMode mode)
        {
            if (!Directory.Exists(options.Value.ZippedAudioBooksLocation))
            {
                Directory.CreateDirectory(options.Value.ZippedAudioBooksLocation);
            }

            return new FileStream(targetPath, mode);
        }


        public Task UnzipAudioBook(string audioBookId)
        {
            try
            {
                var audioBookZipPath = GetAudioBookZipPath(audioBookId);
                if (!System.IO.File.Exists(audioBookZipPath))
                {
                    throw new ArgumentException("Provided zip could not be found!");
                }

                var audioBookDirectory = GetAudioBookDirectory(audioBookId);
                if (Directory.Exists(audioBookDirectory))
                {
                    Directory.Delete(audioBookZipPath, true);
                }

                Directory.CreateDirectory(audioBookDirectory);


                var zipStream = GetAudioBookZipStream(audioBookZipPath, FileMode.Open);
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                archive.ExtractToDirectory(audioBookDirectory, true);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string GetAudioBookDirectory(string audioBookId)
        {
            var targetDirectoryName = audioBookId;
            var finalDirectoryPath = Path.Combine(options.Value.AudioBooksLocation, targetDirectoryName);
            return finalDirectoryPath;
        }

        private string GetAudioBookZipPath(string audioBookId)
        {
            var targetFileName = audioBookId;
            var fileFilePath = Path.Combine(options.Value.ZippedAudioBooksLocation, targetFileName);
            return $"{fileFilePath}.zip";
        }
    }
}
