using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing.Abstractions;

namespace DotCast.Storage.Processing.Steps.UpdateMetadata
{
    public class UpdateMetadataProcessingStep(IStorage storage, IFilesystemPathManager filesystemPathManager) : IProcessingStep
    {
        public async Task<Dictionary<string, ModificationType>> Process(string audioBookId, Dictionary<string, ModificationType> modifiedFiles)
        {
            var needsMetadataUpdate = modifiedFiles.Select(t => (FilePath: t.Key, ModificationType: t.Value)).Any(t => !filesystemPathManager.IsArchive(t.FilePath));
            if (!needsMetadataUpdate)
            {
                return modifiedFiles;
            }

            var audioBook = await storage.ExtractMetadataAsync(audioBookId);
            await storage.UpdateMetadataAsync(audioBook);

            return modifiedFiles;
        }
    }
}