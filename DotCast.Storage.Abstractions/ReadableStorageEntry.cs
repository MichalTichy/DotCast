namespace DotCast.Storage.Abstractions
{
    public record ReadableStorageEntry(string Id, Stream Stream, string MimeType) : StorageEntry(Id);
}