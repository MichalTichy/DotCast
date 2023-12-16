using DotCast.Infrastructure.MetadataManager;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Options;

namespace DotCast.Storage.Storage
{
    internal class SimpleStorage(IOptions<StorageOptions> storageOptions, IMetadataManager metadataManager, IStorageApiInformationProvider apiInformationProvider) : IStorage
    {
        public IEnumerable<StorageEntry> GetEntriesAsync()
        {
            var booksLocation = storageOptions.Value.AudioBooksLocation;

            var books = Directory.EnumerateDirectories(booksLocation);

            foreach (var book in books)
            {
                var id = Path.GetDirectoryName(book);
                if (id != null)
                {
                    yield return new StorageEntry(id);
                }
            }
        }

        public async Task<AudioBook> ExtractMetadataAsync(string id)
        {
            var audioBookStorageEntry = GetStorageEntry(id);
            if (audioBookStorageEntry == null)
            {
                throw new ArgumentException($"AudioBook with id {id} does not exist");
            }

            return await metadataManager.ExtractMetadata(audioBookStorageEntry);
        }

        public async Task UpdateMetadataAsync(AudioBook audioBook)
        {
            var source = GetStorageEntry(audioBook.Id);
            if (source == null)
            {
                throw new ArgumentException($"AudioBook with id {audioBook.Id} does not exist");
            }

            await metadataManager.UpdateMetadata(audioBook, source);
        }

        public StorageEntryWithFiles? GetStorageEntry(string id)
        {
            var valueAudioBooksLocation = storageOptions.Value.AudioBooksLocation;
            var audioBookDirectory = Path.Combine(valueAudioBooksLocation, id);
            var directoryInfo = new DirectoryInfo(audioBookDirectory);
            if (!directoryInfo.Exists)
            {
                return null;
            }

            var files = directoryInfo.EnumerateFiles().Select(t => new LocalFileInfo(t.FullName, apiInformationProvider.GetFileUrl(t.Directory!.Name, t.Name))).ToList();
            return new StorageEntryWithFiles(id, files);
        }
    }
}