namespace DotCast.Storage.Processing.Steps.MP4A
{
    public interface IMp4ASplitter
    {
        Task<ICollection<string>> SplitAsync(string source, string destination);
    }
}