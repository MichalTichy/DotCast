﻿using DotCast.Infrastructure.FileNameNormalization;
using DotCast.Infrastructure.MetadataManager;
using DotCast.Infrastructure.MimeType;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Logging;

namespace DotCast.Storage.Storage
{
    internal class SimpleStorage(IFilesystemPathManager filesystemPathManager,
        IMetadataManager metadataManager,
        IStorageApiInformationProvider apiInformationProvider,
        IFileNameNormalizer fileNameNormalizer,
        ILogger<SimpleStorage> logger) : IStorage
    {
        public Task<LocalFileInfo> RenameFileAsync(string id, LocalFileInfo fileInfo, string newName, CancellationToken cancellationToken = default)
        {
            var currentFileName = Path.GetFileName(fileInfo.LocalPath);
            var newPath = filesystemPathManager.GetTargetFilePath(id, newName);
            File.Move(fileInfo.LocalPath, newPath);
            var localFileInfo = new LocalFileInfo(newPath, apiInformationProvider.GetFileUrl(id, newName, filesystemPathManager.IsArchive(currentFileName)));
            return Task.FromResult(localFileInfo);
        }

        public async Task<LocalFileInfo> StoreAsync(Stream stream, string audioBookId, string fileName, CancellationToken cancellationToken)
        {
            var newFileName = GetFileName(audioBookId, fileName);

            var filePath = filesystemPathManager.GetTargetFilePath(audioBookId, newFileName);
            await using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream, cancellationToken);
            var remotePath = apiInformationProvider.GetFileUrl(audioBookId, newFileName, filesystemPathManager.IsArchive(newFileName));

            return new LocalFileInfo(filePath, remotePath);
        }

        private string GetFileName(string audiobookId, string fileName)
        {
            if (filesystemPathManager.IsArchive(fileName))
            {
                var extension = Path.GetExtension(fileName);
                return $"{audiobookId}{extension}";
            }

            return fileNameNormalizer.Normalize(fileName);
        }

        public IEnumerable<StorageEntry> GetEntries()
        {
            var booksLocation = filesystemPathManager.GetAudioBooksLocation();

            var books = Directory.EnumerateDirectories(booksLocation).Select(Path.GetFileName).ToArray();

            var archivedBooksLocation = filesystemPathManager.GetAudioBooksZipDirectoryLocation();
            var archives = Directory.EnumerateFiles(archivedBooksLocation).Select(Path.GetFileNameWithoutExtension).ToArray();

            var storageEntries = books.Union(archives).Distinct().Select(item => new StorageEntry(item!));
            foreach (var entry in storageEntries)
            {
                yield return entry;
            }
        }

        public async Task<AudioBookInfo> ExtractMetadataAsync(string id, CancellationToken cancellationToken = default)
        {
            var audioBookStorageEntry = GetStorageEntry(id);
            if (audioBookStorageEntry == null)
            {
                throw new ArgumentException($"AudioBook with id {id} does not exist");
            }

            return await metadataManager.ExtractMetadata(audioBookStorageEntry, cancellationToken);
        }

        public async Task UpdateMetadataAsync(AudioBookInfo audioBook, CancellationToken cancellationToken = default)
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
                logger.LogWarning($"No files found for audiobook {id}. Extracted path {audioBookFilesDirectory} | archive path {audioBookZipPath}");
                return null;
            }

            LocalFileInfo? zipFileInfo = null;
            if (hasZipVersion)
            {
                zipFileInfo = new LocalFileInfo(audioBookZipPath, apiInformationProvider.GetFileUrl(id, archiveName, true));
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

            return PrepareReadableStorageEntry(archive, archivesLocation);
        }
    }
}