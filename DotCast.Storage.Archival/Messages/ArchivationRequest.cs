using Wolverine.Attributes;

namespace DotCast.Storage.Archival.Messages
{
    [LocalQueue(ArchivationWorker.QueueName)]
    public record ArchivationRequest(string AudioBookId);
}