namespace DotCast.AudioBookProvider.Base
{
    public interface IAudioBookUploader
    {
        FileStream GetAudioBookFileWriteStream(string audioBookName, string fileName, string fileContentType, out string audioBookId);
        FileStream GetAudioBookZipWriteStream(string audioBookName, out string audioBookId);
        Task UnzipAudioBook(string audioBookId);
    }
}
