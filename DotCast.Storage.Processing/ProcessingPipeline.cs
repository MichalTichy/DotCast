using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Ardalis.GuardClauses;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing.Abstractions;
using Wolverine;
using Wolverine.Attributes;

namespace DotCast.Storage.Processing
{
    [WolverineHandler]
    public sealed class ProcessingPipeline(ICollection<IProcessingStep> steps, IMessageBus messageBus, IStorage storage) : IMessageHandler<FilesModificationsFinished>
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

        public IReadOnlyCollection<IProcessingStep> Steps { get; } = new ReadOnlyCollection<IProcessingStep>(steps.ToList());

        public async Task Process(StorageEntryWithFiles source, ICollection<string> modifiedFiles)
        {
            var lockKey = source.Id;
            var semaphore = Locks.GetOrAdd(lockKey, k => new SemaphoreSlim(1, 1));

            var lockTaken = await semaphore.WaitAsync(0);
            if (!lockTaken)
            {
                throw new InvalidOperationException($"Processing is already running for StorageEntry ID: {lockKey}");
            }

            try
            {
                var modifications = modifiedFiles;
                foreach (var step in steps)
                {
                    modifications = await step.Process(source.Id, source.Archive != null, modifications);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Handle(FilesModificationsFinished message)
        {
            var entry = storage.GetStorageEntry(message.AudioBookId);
            Guard.Against.Null(entry, message.AudioBookId);

            await Process(entry, message.ModifiedFiles);

            var audioBook = await storage.ExtractMetadataAsync(message.AudioBookId);
            await messageBus.PublishAsync(new AudioBookStorageMetadataUpdated(audioBook));
        }
    }
}
