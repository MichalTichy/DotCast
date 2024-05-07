using DotCast.SharedKernel.Models;

namespace DotCast.Storage.Abstractions
{
    public interface IStorage
    {
        Task<LocalFileInfo> RenameFileAsync(string id, LocalFileInfo fileInfo, string newName, CancellationToken cancellationToken = default);
        Task<LocalFileInfo> StoreAsync(Stream stream, string audioBookId, string fileName, CancellationToken cancellationToken = default);
        IEnumerable<StorageEntry> GetEntries();
        Task<AudioBookInfo> ExtractMetadataAsync(string id, CancellationToken cancellationToken = default);
        Task UpdateMetadataAsync(AudioBookInfo audioBook, CancellationToken cancellationToken = default);
        ReadableStorageEntry? GetFileForRead(string audioBookId, string fileName);
        StorageEntryWithFiles? GetStorageEntry(string id);
        ReadableStorageEntry? GetArchiveForRead(string audioBookId);
    }
}
