using DotCast.Infrastructure.Archival;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing.Abstractions;

namespace DotCast.Storage.Processing.Steps.Zip
{
    public class ZipProcessingStep(IArchiveManager archiveManager, IFilesystemPathManager filesystemPathManager) : IProcessingStep
    {
        public async Task<ICollection<string>> Process(string audioBookId, bool wasArchived, ICollection<string> modifiedFiles)
        {
            var needsRepacking = modifiedFiles.Any(filesystemPathManager.IsArchive) || !wasArchived;
            if (!needsRepacking)
            {
                return modifiedFiles;
            }

            var archivePath = filesystemPathManager.GetTargetFilePath(audioBookId, $"{audioBookId}.zip");
            var sourcePath = filesystemPathManager.GetAudioBookLocation(audioBookId);

            await archiveManager.ZipAsync(sourcePath, archivePath);

            return modifiedFiles.Append(archivePath).ToList();
        }
    }
}
