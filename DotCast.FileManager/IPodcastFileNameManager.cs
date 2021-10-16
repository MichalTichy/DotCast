namespace DotCast.FileManager
{
    public interface IPodcastFileNameManager
    {
        void RenameFilesToUrlFriendlyNames(string targetDirectory, int? minimumDirectoryInactivityTimeInMinutes = 10);
    }
}