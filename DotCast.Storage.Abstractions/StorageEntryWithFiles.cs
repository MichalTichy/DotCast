﻿namespace DotCast.Storage.Abstractions
{
    public record StorageEntryWithFiles(string Id, IReadOnlyCollection<LocalFileInfo> Files, LocalFileInfo? Archive) : StorageEntry(Id);
}