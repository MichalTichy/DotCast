namespace DotCast.Library
{
    public interface ILibraryApiInformationProvider
    {
        Task<string> GetFeedUrlAsync(string audioBookId);
    }
}