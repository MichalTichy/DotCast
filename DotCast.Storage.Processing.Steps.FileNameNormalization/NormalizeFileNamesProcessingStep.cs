using DotCast.Infrastructure.FileNameNormalization;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing.Abstractions;

namespace DotCast.Storage.Processing.Steps.FileNameNormalization
{
    public class NormalizeFileNamesProcessingStep(IStorage storage, IFileNameNormalizer fileNameNormalizer) : IProcessingStep
    {
        public async Task<Dictionary<string, ModificationType>> Process(string audioBookId, Dictionary<string, ModificationType> modifiedFiles)
        {
            var notNormalizedFiles = modifiedFiles.ToList().Where(t => t.Value == ModificationType.Modified).Where(t => !fileNameNormalizer.IsNormalized(t.Key));
            foreach (var notNormalizedFile in notNormalizedFiles)
            {
                var normalizedFileName = fileNameNormalizer.Normalize(notNormalizedFile.Key);
                var normalizedFileInfo = await storage.RenameFileAsync(audioBookId, new LocalFileInfo(notNormalizedFile.Key, string.Empty), normalizedFileName);
                modifiedFiles.Remove(notNormalizedFile.Key);
                modifiedFiles.Add(normalizedFileInfo.LocalPath, notNormalizedFile.Value);
            }

            return modifiedFiles;
        }
    }
}