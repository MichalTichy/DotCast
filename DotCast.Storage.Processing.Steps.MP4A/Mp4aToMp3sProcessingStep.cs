using DotCast.Storage.Processing.Abstractions;
using DotCast.Storage.Abstractions;

namespace DotCast.Storage.Processing.Steps.MP4A
{
    public class Mp4aToMp3sProcessingStep(IMp4ASplitter splitter, IFilesystemPathManager filesystemPathManager) : IProcessingStep
    {
        public async Task<Dictionary<string, ModificationType>> Process(string audioBookId, Dictionary<string, ModificationType> modifiedFiles)
        {
            var existingFiles = modifiedFiles.Where(t => !t.Value.HasFlag(ModificationType.Deleted));
            var mp4AFiles = existingFiles.Where(t => Path.GetExtension(t.Key) == ".m4a").Select(t => t.Key).ToArray();
            if (mp4AFiles.Length == 0)
            {
                return modifiedFiles;
            }

            var destination = filesystemPathManager.GetAudioBookLocation(audioBookId);
            foreach (var mp4AFile in mp4AFiles)
            {
                var newFiles = await splitter.SplitAsync(mp4AFile, destination);
                File.Delete(mp4AFile);
                modifiedFiles[mp4AFile] = ModificationType.Deleted;
                foreach (var newFile in newFiles)
                {
                    modifiedFiles[newFile] = ModificationType.Extracted;
                }
            }

            return modifiedFiles;
        }
    }
}
