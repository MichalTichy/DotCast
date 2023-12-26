using DotCast.Infrastructure.MetadataManager;
using DotCast.Infrastructure.MimeType;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;
using Wolverine;

namespace DotCast.Storage.Storage
{
    internal class SimpleStorage(IFilesystemPathManager filesystemPathManager,
        IMetadataManager metadataManager,
        IStorageApiInformationProvider apiInformationProvider) : IStorage
    {
        public async Task<LocalFileInfo> StoreAsync(Stream stream, string audioBookId, string fileName, CancellationToken cancellationToken)
        {
            var newFileName = GetFileName(audioBookId, fileName);

            var filePath = filesystemPathManager.GetTargetFilePath(audioBookId, fileName);
            await using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream, cancellationToken);
            var remotePath = apiInformationProvider.GetFileUrl(audioBookId, newFileName, filesystemPathManager.IsArchive(fileName));

            return new LocalFileInfo(filePath, remotePath);
        }

        private string GetFileName(string audiobookId, string fileName)
        {
            if (filesystemPathManager.IsArchive(fileName))
            {
                return audiobookId;
            }

            return fileName.Replace(" ", "_");
        }

        public IEnumerable<StorageEntry> GetEntriesAsync()
        {
            var booksLocation = filesystemPathManager.GetAudioBooksLocation();

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
            var audioBookFilesDirectory = new DirectoryInfo(filesystemPathManager.GetAudioBookLocation(id));
            var hasExtractedVersion = audioBookFilesDirectory.Exists;

            var archiveName = $"{id}.zip";
            var audioBookZipPath = filesystemPathManager.GetTargetFilePath(id, archiveName);
            var hasZipVersion = File.Exists(audioBookZipPath);

            if (!hasZipVersion && !hasExtractedVersion)
            {
                return null;
            }

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
            var filePath = filesystemPathManager.GetTargetFilePath(audioBookId, fileName);
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
            var archive = $"{audioBookId}.zip";
            var archivesLocation = filesystemPathManager.GetTargetFilePath(audioBookId, archive);
            if (!File.Exists(archivesLocation))
            {
                return null;
            }

            return PrepareReadableStorageEntry(audioBookId, archivesLocation);
        }
    }
}