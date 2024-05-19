using DotCast.Infrastructure.Archival;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing.Abstractions;
using System.Linq;

namespace DotCast.Storage.Processing.Steps.Unzip
{
    public class UnzipProcessingStep(IArchiveManager archiveManager, IFilesystemPathManager filesystemPathManager) : IProcessingStep
    {
        public async Task<Dictionary<string, ModificationType>> Process(string audioBookId, Dictionary<string, ModificationType> modifiedFiles)
        {
            var archivePath = modifiedFiles.Where(t => !t.Value.HasFlag(ModificationType.Deleted)).Select(t => t.Key).SingleOrDefault(filesystemPathManager.IsArchive);
            if (archivePath is null)
            {
                return modifiedFiles;
            }

            var destinationPath = filesystemPathManager.GetAudioBookLocation(audioBookId);
            var extractedFiles = await archiveManager.UnzipAsync(archivePath, destinationPath);
            foreach (var extractedFile in extractedFiles)
            {
                modifiedFiles[extractedFile] = ModificationType.Extracted;
            }

            return modifiedFiles;
        }
    }
}
