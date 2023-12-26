using DotCast.Storage.Abstractions;

namespace DotCast.Storage.Processing.Abstractions
{
    public interface IProcessingStep
    {
        public Task<ICollection<string>> Process(string audioBookId, bool wasArchived, ICollection<string> modifiedFiles);
    }
}