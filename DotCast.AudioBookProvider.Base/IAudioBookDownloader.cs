namespace DotCast.AudioBookProvider.Base
{
    public interface IAudioBookDownloader
    {
        IEnumerable<string> GetAudioBookIdsAvailableForDownload();

        bool IsDownloadSupported(string audioBookId);
        Task<string> GetZipDownloadUrl(string audioBookId);
        Task GenerateZipForDownload(string audioBookId, bool replace = false);
    }
}
