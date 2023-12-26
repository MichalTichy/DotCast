namespace DotCast.Infrastructure.Archival
{
    public interface IArchiveManager
    {
        Task<ICollection<string>> UnzipAsync(string archivePath, string destinationPath, CancellationToken cancellationToken = default);
        Task ZipAsync(string sourcePath, string archivePath, CancellationToken cancellationToken = default);
    }
}