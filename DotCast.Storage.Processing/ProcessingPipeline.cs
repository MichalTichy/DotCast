﻿using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Ardalis.GuardClauses;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing.Abstractions;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;

namespace DotCast.Storage.Processing
{
    [WolverineHandler]
    public sealed class ProcessingPipeline
        (ICollection<IProcessingStep> steps, IMessagePublisher messenger, IStorage storage, ILogger<ProcessingPipeline> logger) : IMessageHandler<AudioBookReadyForProcessing>
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

        public IReadOnlyCollection<IProcessingStep> Steps => new ReadOnlyCollection<IProcessingStep>(steps.ToList());

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
                await messenger.PublishAsync(new ProcessingStatusChanged(Locks.Keys));
                logger.LogInformation($"Started processing for {source.Id}. Currently running {Locks.Count} processings.");
                var modifications = modifiedFiles.ToDictionary(t => t, s => ModificationType.FileContentModified);
                foreach (var step in steps)
                {
                    modifications = await step.Process(source.Id, modifications);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Processing for {source.Id} failed.");
            }
            finally
            {
                Locks.TryRemove(lockKey, out _);
                semaphore.Release();
                logger.LogInformation($"Finished processing for {source.Id}. Currently running {Locks.Count} processings.");
                await messenger.PublishAsync(new ProcessingStatusChanged(Locks.Keys));
            }
        }

        public async Task Handle(AudioBookReadyForProcessing message)
        {
            var entry = storage.GetStorageEntry(message.AudioBookId);
            Guard.Against.Null(entry, message.AudioBookId);

            await Process(entry, message.ModifiedFiles);

            var audioBook = await storage.ExtractMetadataAsync(message.AudioBookId);
            await messenger.PublishAsync(new AudioBookStorageMetadataUpdated(audioBook));
        }
    }
}
