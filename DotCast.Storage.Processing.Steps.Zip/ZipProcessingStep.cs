using DotCast.Infrastructure.Archival;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing.Abstractions;

namespace DotCast.Storage.Processing.Steps.Zip
{
    public class ZipProcessingStep(IArchiveManager archiveManager, IFilesystemPathManager filesystemPathManager) : IProcessingStep
    {
        public async Task<Dictionary<string, ModificationType>> Process(string audioBookId, Dictionary<string, ModificationType> modifiedFiles)
        {
            var needsRepacking = modifiedFiles.Select(t => (FilePath: t.Key, ModificationType: t.Value)).Where(t =>
                    t.ModificationType.HasFlag(ModificationType.Deleted) ||
                    t.ModificationType.HasFlag(ModificationType.FileContentModified) ||
                    t.ModificationType.HasFlag(ModificationType.Renamed))
                .Any(t => !filesystemPathManager.IsArchive(t.FilePath));
            if (!needsRepacking)
            {
                return modifiedFiles;
            }

            var archivePath = filesystemPathManager.GetTargetFilePath(audioBookId, $"{audioBookId}.zip");
            var sourcePath = filesystemPathManager.GetAudioBookLocation(audioBookId);
            await archiveManager.ZipAsync(sourcePath, archivePath);
            modifiedFiles[archivePath] = ModificationType.FileContentModified;

            return modifiedFiles;
        }
    }
}