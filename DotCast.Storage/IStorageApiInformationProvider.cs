namespace DotCast.Storage
{
    public interface IStorageApiInformationProvider
    {
        string GetFileUrl(string audioBookId, string fileName, bool isArchive);
    }
}