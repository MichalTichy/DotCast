namespace DotCast.Storage.Processing.Abstractions
{
    public interface IProcessingStep
    {
        public Task<Dictionary<string, ModificationType>> Process(string audioBookId, Dictionary<string, ModificationType> modifiedFiles);
    }
}