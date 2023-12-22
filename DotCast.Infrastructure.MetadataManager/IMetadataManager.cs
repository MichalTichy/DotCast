using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;

namespace DotCast.Infrastructure.MetadataManager
{
    public interface IMetadataManager
    {
        Task<AudioBook> ExtractMetadata(StorageEntryWithFiles source, CancellationToken cancellationToken = default);
        Task UpdateMetadata(AudioBook audioBook, StorageEntryWithFiles source, CancellationToken cancellationToke = default);
    }
}