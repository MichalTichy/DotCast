using System.Collections.Concurrent;
using Ardalis.GuardClauses;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using Wolverine;
using Wolverine.Attributes;

namespace DotCast.Storage.Processing
{
    public class FileUploadTracker(IMessageBus messageBus, IFilesystemPathManager filesystemPathManager) :
        IMessageHandler<AudioBookUploadStartRequest>,
        IMessageHandler<FileUploaded>
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> RunningUploads = new();

        public Task Handle(AudioBookUploadStartRequest message)
        {
            var newFilesWaitingForUpload = message.Files;
            var pendingUploads = RunningUploads.GetOrAdd(message.AudioBookId, s => new ConcurrentDictionary<string, bool>());
            foreach (var file in newFilesWaitingForUpload)
            {
                pendingUploads.TryAdd(file, false);
            }

            return Task.CompletedTask;
        }

        private static readonly SemaphoreSlim IsDoneCheckLock = new(1, 1);

        public async Task Handle(FileUploaded message)
        {
            var hasRunningUpload = RunningUploads.TryGetValue(message.AudioBookId, out var pendingUploads);
            if (!hasRunningUpload)
            {
                return;
            }

            pendingUploads![message.FileName] = true;

            await IsDoneCheckLock.WaitAsync();

            if (!hasRunningUpload)
            {
                return;
            }

            try
            {
                var uploads = pendingUploads.ToList();
                var isDone = uploads.All(t => t.Value);
                if (isDone)
                {
                    var newFiles = uploads.Select(t => filesystemPathManager.GetTargetFilePath(message.AudioBookId, t.Key)).ToArray();
                    RunningUploads.TryRemove(message.AudioBookId, out _);

                    var newMessage = new FilesModificationsFinished(message.AudioBookId, newFiles);
                    await messageBus.PublishAsync(newMessage);
                }
            }
            finally
            {
                IsDoneCheckLock.Release();
            }
        }
    }
}
