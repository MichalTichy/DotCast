using System.Collections.Concurrent;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;

namespace DotCast.Storage.Processing
{
    public class FileUploadTracker(IMessagePublisher messenger, IFilesystemPathManager filesystemPathManager) :
        IMessageHandler<AudioBookUploadStartRequest>,
        IMessageHandler<FileUploaded>
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string?>> RunningUploads = new();

        public Task Handle(AudioBookUploadStartRequest message)
        {
            var newFilesWaitingForUpload = message.Files;
            var pendingUploads = RunningUploads.GetOrAdd(message.AudioBookId, s => new ConcurrentDictionary<string, string?>());
            foreach (var file in newFilesWaitingForUpload)
            {
                pendingUploads.TryAdd(file, null);
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

            pendingUploads![message.OriginalFilename] = message.NewFileName;

            await IsDoneCheckLock.WaitAsync();

            if (!hasRunningUpload)
            {
                return;
            }

            try
            {
                var uploads = pendingUploads.ToList();
                var isDone = uploads.All(t => t.Value != null);
                if (isDone)
                {
                    var newFiles = uploads.Select(t => filesystemPathManager.GetTargetFilePath(message.AudioBookId, t.Value!)).ToArray();
                    RunningUploads.TryRemove(message.AudioBookId, out _);

                    var newMessage = new AudioBookReadyForProcessing(message.AudioBookId, newFiles);
                    await messenger.PublishAsync(newMessage);
                }
            }
            finally
            {
                IsDoneCheckLock.Release();
            }
        }
    }
}
