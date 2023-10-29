namespace DotCast.AudioBookProvider.FileSystem
{
    public class FileSystemAudioBookProviderOptions
    {
        public string AudioBooksLocation { get; set; } = null!;
        public string ZippedAudioBooksLocation { get; set; } = null!;
        public string AudioBookServerUrl { get; set; } = null!;
    }
}
