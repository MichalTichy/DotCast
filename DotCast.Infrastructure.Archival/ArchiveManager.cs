using System.IO.Compression;

namespace DotCast.Infrastructure.Archival
{
    public class ArchiveManager : IArchiveManager
    {
        public async Task<ICollection<string>> UnzipAsync(string archivePath, string destinationPath, CancellationToken cancellationToken = default)
        {
            var files = new List<string>();
            try
            {
                Directory.CreateDirectory(destinationPath);

                using var archive = ZipFile.OpenRead(archivePath);
                foreach (var entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var isDirectory = string.IsNullOrWhiteSpace(entry.Name);
                    if (isDirectory)
                    {
                        continue;
                    }

                    var destinationFilePath = Path.Combine(destinationPath, entry.Name);
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath)!);

                    await using var inputStream = entry.Open();
                    await using var outputStream = File.Create(destinationFilePath);
                    await inputStream.CopyToAsync(outputStream, 81920, cancellationToken);
                    files.Add(destinationFilePath);
                }
            }
            catch (OperationCanceledException)
            {
                throw; // rethrow to preserve stack details
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error unzipping file: {archivePath}", ex);
            }

            return files;
        }

        public async Task ZipAsync(string sourcePath, string archivePath, CancellationToken cancellationToken = default)
        {
            var tempArchivePath = $"{archivePath}.tmp";

            try
            {
                using (var archive = ZipFile.Open(tempArchivePath, ZipArchiveMode.Create))
                {
                    var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var relativePath = file.Substring(sourcePath.Length + 1);
                        archive.CreateEntryFromFile(file, relativePath);
                    }
                }

                if (File.Exists(archivePath))
                {
                    File.Delete(archivePath);
                }

                File.Move(tempArchivePath, archivePath);

                await Task.CompletedTask; // Placeholder to mimic async work
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(tempArchivePath))
                {
                    File.Delete(tempArchivePath);
                }
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error zipping file: {sourcePath}", ex);
            }
        }
    }
}
