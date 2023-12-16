using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;

namespace DotCast.Storage.Storage
{
    internal interface IStorage
    {
        public IEnumerable<StorageEntry> GetEntriesAsync();
        public Task<AudioBook> ExtractMetadataAsync(string id);
        Task UpdateMetadataAsync(AudioBook audioBook);
        StorageEntryWithFiles? GetStorageEntry(string id);
    }
}
