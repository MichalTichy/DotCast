namespace DotCast.Storage.Abstractions
{
    public interface IFilesystemPathManager
    {
        string GetTargetFilePath(string audioBookId, string fileName);
        bool IsArchive(string filePath);
        string GetAudioBooksLocation();
        string GetAudioBooksZipDirectoryLocation();
        string GetAudioBookLocation(string audiobookId);
    }
}
