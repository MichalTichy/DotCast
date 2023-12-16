using DotCast.SharedKernel.Models;

namespace DotCast.SharedKernel.Messages
{
    public record AudioBooksRetrievalRequest(string? Filter = null);

    public record AudioBookDetailRequest(string Id);

    public record AudioBookMetadataUpdated(AudioBook AudioBook);


    public record NewAudioBookRequest(string Name, AudioBook? AudioBook = null);

    public record AudioBookRssRequest(string Id);

    public record AudioBookUploadStartRequest(string AudioBookId, ICollection<string> Files);

    public record AudioBooksStatisticsRequest;

    public record RestoreFromFileSystemRequest;

    public record AudiobookInfoSuggestionsRequest(string Name, int? Count = null);

    public record AudioBookMetadataRequest(string Id);
}