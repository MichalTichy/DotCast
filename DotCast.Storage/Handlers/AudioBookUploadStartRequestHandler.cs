using DotCast.Infrastructure.PresignedUrls;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;

namespace DotCast.Storage.Handlers
{
    public class AudioBookUploadStartRequestHandler
    (
        IStorage storage,
        IPresignedUrlManager presignedUrlManager,
        IStorageApiInformationProvider apiInformationProvider
    ) : IMessageHandler<AudioBookUploadStartRequest, IReadOnlyCollection<PreuploadFileInformation>>
    {
        public Task<IReadOnlyCollection<PreuploadFileInformation>> Handle(AudioBookUploadStartRequest message)
        {
            var result = new List<PreuploadFileInformation>();
            var currentFiles = storage.GetStorageEntry(message.AudioBookId)?.Files.Select(t => Path.GetFileName(t.LocalPath)).ToArray();
            foreach (var messageFile in message.Files)
            {
                var isArchive = messageFile.EndsWith(".zip");
                var url = apiInformationProvider.GetFileUrl(message.AudioBookId, messageFile, isArchive);
                var signedUrl = presignedUrlManager.GenerateUrl(url);
                var exists = currentFiles != null && currentFiles.Contains(messageFile);
                result.Add(new PreuploadFileInformation(messageFile, signedUrl, exists));
            }

            return Task.FromResult<IReadOnlyCollection<PreuploadFileInformation>>(result.AsReadOnly());
        }
    }
}