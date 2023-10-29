namespace DotCast.AudioBookProvider.Base
{
    public interface IAudioBookUploader
    {
        FileStream GetAudioBookFileWriteStream(string AudioBookName, string fileName, string fileContentType, out string AudioBookId);
        FileStream GetAudioBookZipWriteStream(string AudioBookName, out string AudioBookId);
        Task UnzipAudioBook(string AudioBookId);
    }
}
