using DotCast.Infrastructure.MetadataManager;
using DotCast.Infrastructure.MimeType;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Options;
using Wolverine;

namespace DotCast.Storage.Storage
{
    internal class SimpleStorage(IOptions<StorageOptions> storageOptions, IMetadataManager metadataManager, IStorageApiInformationProvider apiInformationProvider) : IStorage
    {
        public async Task<LocalFileInfo> StoreAsync(Stream stream, string audioBookId, string fileName, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(fileName);
            var isArchive = IsArchive(extension);
            var newFileName = GetFileName(audioBookId, fileName, extension);

            var filePath = GetTargetFilePath(audioBookId, newFileName, extension);
            await using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream, cancellationToken);
            var remotePath = apiInformationProvider.GetFileUrl(audioBookId, newFileName, isArchive);
            return new LocalFileInfo(filePath, remotePath);
        }

        private bool IsArchive(string extension)
        {
            return extension == ".zip";
        }

        private string GetTargetFilePath(string audioBookId, string fileName, string extension)
        {
            string targetDirectory;
            if (IsArchive(extension))
            {
                targetDirectory = storageOptions.Value.ZippedAudioBooksLocation;
            }
            else
            {
                var audioBooksLocation = storageOptions.Value.AudioBooksLocation;
                targetDirectory = Path.Combine(audioBooksLocation, audioBookId);
            }

            Directory.CreateDirectory(targetDirectory);
            var filePath = Path.Combine(targetDirectory, fileName);
            return filePath;
        }

        private string GetFileName(string audiobookId, string fileName, string extension)
        {
            if (IsArchive(extension))
            {
                return audiobookId;
            }

            return fileName.Replace(" ", "_");
        }

        public IEnumerable<StorageEntry> GetEntriesAsync()
        {
            var booksLocation = storageOptions.Value.AudioBooksLocation;

            var books = Directory.EnumerateDirectories(booksLocation);

            foreach (var book in books)
            {
                var id = Path.GetFileName(book);
                yield return new StorageEntry(id);
            }
        }

        public async Task<AudioBook> ExtractMetadataAsync(string id, CancellationToken cancellationToken = default)
        {
            var audioBookStorageEntry = GetStorageEntry(id);
            if (audioBookStorageEntry == null)
            {
                throw new ArgumentException($"AudioBook with id {id} does not exist");
            }

            return await metadataManager.ExtractMetadata(audioBookStorageEntry, cancellationToken);
        }

        public async Task UpdateMetadataAsync(AudioBook audioBook, CancellationToken cancellationToken = default)
        {
            var source = GetStorageEntry(audioBook.Id);
            if (source == null)
            {
                throw new ArgumentException($"AudioBook with id {audioBook.Id} does not exist");
            }

            await metadataManager.UpdateMetadata(audioBook, source, cancellationToken);
        }

        public StorageEntryWithFiles? GetStorageEntry(string id)
        {
            var audioBooksLocation = storageOptions.Value.AudioBooksLocation;
            var audioBookDirectory = Path.Combine(audioBooksLocation, id);
            var audioBookFilesDirectory = new DirectoryInfo(audioBookDirectory);
            var hasExtractedVersion = audioBookFilesDirectory.Exists;
            var audioBooksZipLocation = storageOptions.Value.ZippedAudioBooksLocation;
            var archiveName = $"{id}.zip";
            var audioBookZipPath = Path.Combine(audioBooksZipLocation, archiveName);
            var hasZipVersion = File.Exists(audioBookZipPath);
            LocalFileInfo? zipFileInfo = null;
            if (hasZipVersion)
            {
                apiInformationProvider.GetFileUrl(id, archiveName, true);
            }

            var files = hasExtractedVersion
                ? audioBookFilesDirectory.EnumerateFiles()
                    .Select(t =>
                    {
                        var localPath = t.FullName;
                        var remotePath = apiInformationProvider.GetFileUrl(t.Directory!.Name, t.Name, false);

                        return new LocalFileInfo(localPath, remotePath);
                    }).ToList()
                : new List<LocalFileInfo>(0);
            return new StorageEntryWithFiles(id, files, zipFileInfo);
        }

        public ReadableStorageEntry? GetFileForRead(string audioBookId, string fileName)
        {
            var audioBooksLocation = storageOptions.Value.AudioBooksLocation;
            var audioBookDirectory = Path.Combine(audioBooksLocation, audioBookId);
            var filePath = Path.Combine(audioBookDirectory, fileName);
            if (!File.Exists(filePath))
            {
                return null;
            }

            return PrepareReadableStorageEntry($"{audioBookId}/{fileName}", filePath);
        }

        private static ReadableStorageEntry PrepareReadableStorageEntry(string fileId, string filePath)
        {
            var mimeType = MimeTypeExtractor.GetMimeType(filePath);
            var stream = File.OpenRead(filePath);
            return new ReadableStorageEntry(fileId, stream, mimeType);
        }

        public ReadableStorageEntry? GetArchiveForRead(string audioBookId)
        {
            var archivesDirectory = storageOptions.Value.ZippedAudioBooksLocation;
            var archiveLocation = Path.Combine(archivesDirectory, $"{audioBookId}.zip");
            if (!File.Exists(archiveLocation))
            {
                return null;
            }

            return PrepareReadableStorageEntry(audioBookId, archiveLocation);
        }
    }
}