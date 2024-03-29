﻿using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;

namespace DotCast.Infrastructure.MetadataManager
{
    public interface IMetadataManager
    {
        Task<AudioBookInfo> ExtractMetadata(StorageEntryWithFiles source, CancellationToken cancellationToken = default);
        Task UpdateMetadata(AudioBookInfo audioBook, StorageEntryWithFiles source, CancellationToken cancellationToke = default);
    }
}