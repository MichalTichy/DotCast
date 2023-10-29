using System.Globalization;
using System.IO.Compression;
using System.Text;
using DotCast.AudioBookProvider.Base;
using DotCast.AudioBookProvider.Postgre;
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
            var AudioBookName = GetNormalizedName(id);

            var AudioBookPath = Path.Combine(settings.AudioBooksLocation, id);
            var directory = new DirectoryInfo(AudioBookPath);
            var filePaths = directory.GetFiles().Select(t => new LocalFileInfo(t.FullName, GetFileUrl(t.Directory!.Name, t.Name))).ToArray();

            var rssGeneratorParams = new RssFromFileParams(AudioBookName, filePaths);
            return rssGeneratorParams;
        }

        public async Task<string?> GetFeedCover(string id)
        {
            var rssFromFileParams = GetFeedParams(id);
            var feed = await rssGenerator.BuildFeed(rssFromFileParams);
            return feed.ImageUrl;
        }

        public async IAsyncEnumerable<AudioBookInfo> GetAudioBooks(string? searchText = null)
        {
            var baseDirectory = new DirectoryInfo(settings.AudioBooksLocation);
            foreach (var directory in baseDirectory.GetDirectories())
            {
                var AudioBookInfo = await Get(directory.Name);
                if (AudioBookInfo == null)
                {
                    continue;
                }

                var matchesSearchText = searchText == null ||
                                        AudioBookInfo.Name.Contains(searchText) ||
                                        AudioBookInfo.AuthorName.Contains(searchText) ||
                                        (AudioBookInfo.SeriesName?.Contains(searchText) ?? false);
                if (!matchesSearchText)
                {
                    continue;
                }

                yield return AudioBookInfo;
            }
        }

        public Task UpdateAudioBookInfo(AudioBookInfo AudioBookInfo)
        {
            _ = Task.Run(() =>
            {
                var localFileInfos = GetFiles(AudioBookInfo.Id, out _);
                foreach (var localFileInfo in localFileInfos)
                {
                    try
                    {
                        var file = File.Create(localFileInfo.LocalPath);
                        file.Tag.Album = AudioBookInfo.Name;
                        file.Tag.Performers = new[] {AudioBookInfo.AuthorName};

                        file.Tag.Grouping = AudioBookInfo.SeriesName;
                        file.Tag.TitleSort = AudioBookInfo.OrderInSeries.ToString();

                        file.Tag.Description = AudioBookInfo.Description;

                        file.Save();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            });

            return Task.CompletedTask;
        }

        public async Task<AudioBookInfo?> Get(string id)
        {
            var filePaths = GetFiles(id, out var directory);

            var rssGeneratorParams = new RssFromFileParams(id, filePaths);

            var feed = await rssGenerator.BuildFeed(rssGeneratorParams);

            return new AudioBookInfo(directory.Name, feed.Title, feed.AuthorName ?? "Unknown author", null, 0, feed.Description, $"{settings.AudioBookServerUrl}/AudioBook/{directory.Name}", feed.ImageUrl,
                0,feed.Duration);
        }

        public async Task<AudioBooksStatistics> GetStatistics()
        {
            var AudioBooks = await GetAudioBooks().ToListAsync();
            return AudioBooksStatistics.Create(AudioBooks);
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


        public async Task GenerateZip(string AudioBookId, bool replace = false)
        {
            var path = GetAudioBookZipPath(AudioBookId);
            if (!replace && System.IO.File.Exists(path))
            {
                return;
            }

            var AudioBookDirectory = GetAudioBookDirectory(AudioBookId);
            var files = Directory.GetFiles(AudioBookDirectory);

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


        public bool IsDownloadSupported(string AudioBookId)
        {
            return System.IO.File.Exists(GetAudioBookZipPath(AudioBookId));
        }

        public Task<string> GetZipDownloadUrl(string AudioBookId)
        {
            var AudioBookZipPath = GetAudioBookZipPath(AudioBookId);

            if (!System.IO.File.Exists(AudioBookZipPath))
            {
                throw new ArgumentException("Unable to find requested zip");
            }

            var fileName = Path.GetFileName(AudioBookZipPath);
            return Task.FromResult(GetZipUrl(fileName));
        }

        public FileStream GetAudioBookFileWriteStream(string AudioBookName, string fileName, string fileContentType, out string AudioBookId)
        {
            AudioBookId = GetEscapedName(AudioBookName);
            var finalDirectoryPath = GetAudioBookDirectory(AudioBookId);
            var targetFileName = GetEscapedName(fileName);
            var fileFilePath = Path.Combine(finalDirectoryPath, targetFileName);
            if (!Directory.Exists(finalDirectoryPath))
            {
                Directory.CreateDirectory(finalDirectoryPath);
            }

            return new FileStream(fileFilePath, FileMode.Create);
        }

        public FileStream GetAudioBookZipWriteStream(string AudioBookName, out string AudioBookId)
        {
            AudioBookId = GetEscapedName(AudioBookName);

            var path = GetAudioBookZipPath(AudioBookId);

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


        public Task UnzipAudioBook(string AudioBookId)
        {
            try
            {
                var AudioBookZipPath = GetAudioBookZipPath(AudioBookId);
                if (!System.IO.File.Exists(AudioBookZipPath))
                {
                    throw new ArgumentException("Provided zip could not be found!");
                }

                var AudioBookDirectory = GetAudioBookDirectory(AudioBookId);
                if (Directory.Exists(AudioBookDirectory))
                {
                    Directory.Delete(AudioBookZipPath, true);
                }

                Directory.CreateDirectory(AudioBookDirectory);


                var zipStream = GetAudioBookZipStream(AudioBookZipPath, FileMode.Open);
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                archive.ExtractToDirectory(AudioBookDirectory, true);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string GetAudioBookDirectory(string AudioBookId)
        {
            var targetDirectoryName = AudioBookId;
            var finalDirectoryPath = Path.Combine(options.Value.AudioBooksLocation, targetDirectoryName);
            return finalDirectoryPath;
        }

        private string GetAudioBookZipPath(string AudioBookId)
        {
            var targetFileName = AudioBookId;
            var fileFilePath = Path.Combine(options.Value.ZippedAudioBooksLocation, targetFileName);
            return $"{fileFilePath}.zip";
        }
    }
}
