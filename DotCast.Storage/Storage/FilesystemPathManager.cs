using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Options;

namespace DotCast.Storage.Storage
{
    public class FilesystemPathManager(IOptions<StorageOptions> storageOptions) : IFilesystemPathManager
    {
        public string GetAudioBookLocation(string audiobookId)
        {
            var audioBooksLocation = GetAudioBooksLocation();
            return Path.Combine(audioBooksLocation, audiobookId);
        }

        public string GetAudioBooksLocation()
        {
            return storageOptions.Value.AudioBooksLocation;
        }

        public string GetAudioBooksZipDirectoryLocation()
        {
            return storageOptions.Value.ZippedAudioBooksLocation;
        }

        public string GetTargetFilePath(string audioBookId, string fileName)
        {
            string targetDirectory;
            if (IsArchive(fileName))
            {
                targetDirectory = storageOptions.Value.ZippedAudioBooksLocation;
            }
            else
            {
                var audioBooksLocation = storageOptions.Value.AudioBooksLocation;
                targetDirectory = Path.Combine(audioBooksLocation, audioBookId);
            }

            Directory.CreateDirectory(targetDirectory);
            var filePath = Path.Combine(targetDirectory, fileName);
            return filePath;
        }

        public bool IsArchive(string filePath)
        {
            return filePath.EndsWith(".zip");
        }
    }
}