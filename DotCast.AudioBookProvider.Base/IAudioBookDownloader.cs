namespace DotCast.AudioBookProvider.Base
{
    public interface IAudioBookDownloader
    {
        IEnumerable<string> GetAudioBookIdsAvailableForDownload();

        bool IsDownloadSupported(string AudioBookId);
        Task<string> GetZipDownloadUrl(string AudioBookId);
        Task GenerateZip(string AudioBookId, bool replace = false);
    }
}
