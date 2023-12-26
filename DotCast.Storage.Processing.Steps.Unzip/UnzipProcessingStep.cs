using DotCast.Infrastructure.Archival;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing.Abstractions;

namespace DotCast.Storage.Processing.Steps.Unzip
{
    public class UnzipProcessingStep(IArchiveManager archiveManager, IFilesystemPathManager filesystemPathManager) : IProcessingStep
    {
        public async Task<ICollection<string>> Process(string audioBookId, bool wasArchived, ICollection<string> modifiedFiles)
        {
            var archivePath = modifiedFiles.SingleOrDefault(filesystemPathManager.IsArchive);
            if (archivePath is null)
            {
                return modifiedFiles;
            }

            var destinationPath = filesystemPathManager.GetAudioBookLocation(audioBookId);
            var extractedFiles = await archiveManager.UnzipAsync(archivePath, destinationPath);
            return modifiedFiles.Concat(extractedFiles).Distinct().ToList();
        }
    }
}
