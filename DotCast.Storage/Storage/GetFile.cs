using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;

namespace DotCast.Storage.Storage
{
    public interface IStorage
    {
        public Task<LocalFileInfo> StoreAsync(Stream stream, string audioBookId, string fileName, CancellationToken cancellationToken = default);
        public IEnumerable<StorageEntry> GetEntriesAsync();
        public Task<AudioBook> ExtractMetadataAsync(string id,CancellationToken cancellationToken=default);
        Task UpdateMetadataAsync(AudioBook audioBook, CancellationToken cancellationToken = default);
        ReadableStorageEntry? GetFileForRead(string audioBookId, string fileName);
        StorageEntryWithFiles? GetStorageEntry(string id);
        ReadableStorageEntry? GetArchiveForRead(string audioBookId);
    }
}
