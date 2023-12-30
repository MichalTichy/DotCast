namespace DotCast.Infrastructure.FileNameNormalization
{
    public interface IFileNameNormalizer
    {
        bool IsNormalized(string path);
        string Normalize(string path);
    }
}