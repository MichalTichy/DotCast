using DotCast.SharedKernel.Models;

namespace DotCast.SharedKernel.Messages
{
    //WEB
    public record AudioBooksRetrievalRequest(AudioBookLibraryFilter? Filter = null);

    public record AudioBookLibraryFacetsRequest;

    public record AudioBookDetailRequest(string Id);

    public record AudioBookEdited(AudioBook AudioBook);

    public record AudioBookDeleteRequest(string Id);

    public record NewAudioBookIdRequest(string Name);

    public record AudioBookUploadStartRequest(string AudioBookId, ICollection<string> Files);

    public record AudioBooksStatisticsRequest;

    public record ActivePlaybacksRequest;

    public record BooksToRateRequest;

    public record RateAudioBookRequest(string AudioBookId, int Rating);

    public record ClearAudioBookRatingRequest(string AudioBookId);

    public record SetAudioBookListenedRequest(string AudioBookId, bool Listened);

    public record AudioBookNotFinishedRequest(string AudioBookId);

    public record AudioBookUserStateRequest(IReadOnlyCollection<string> AudioBookIds);

    public record FeaturedAudioBookRequest(AudioBookLibraryFilter? Filter = null);

    public record SetAudioBookInterestRequest(string AudioBookId, bool Interested);

    public record RestoreFromFileSystemRequest;

    public record ReprocessAllAudioBooksRequest(bool Unzip = false);

    public record AudiobookInfoSuggestionsRequest(string Name, int? Count = null, string? AuthorName = null);

    public record ApplyAudiobookSuggestionsToAllRequest;

    public record ApplyAudiobookSuggestionsToAllResult(int Total, int StrongMatches, int Updated, int NoSuggestions, int WeakMatches);


    //STORAGE
    public record AudioBookStorageMetadataUpdated(AudioBookInfo AudioBookInfo);

    public record FileUploaded(string AudioBookId, string OriginalFilename, string NewFileName);

    public record AudioBookReadyForProcessing(string AudioBookId, ICollection<string> ModifiedFiles);

    public record ProcessingStatusChanged(ICollection<string> RunningProcessings);

    //LIBRARY
    public record AudioBookRssRequest(string Id);

    public record AudioBookRssLinkGenerated(string AudioBookId);

    public record AudioBookPlaybackMarkedFinished(string AudioBookId, string UserId, DateTime Timestamp);

    public record AudioBookPlaybackMarkedUnfinished(string AudioBookId, string UserId, DateTime Timestamp);

    public record AudioBookPlaybackNotFinishedConfirmed(string AudioBookId, string UserId, DateTime Timestamp);

    public record AudioBookRssGenerated(string AudioBookId, string UserId, DateTime Timestamp);

    public record FileRead(string AudioBookId, string? UserId, string FileId, DateTime Timestamp);

    public record ArchiveRead(string AudioBookId, string? UserId, DateTime Timestamp);

    public record AudioBookDeleted(string Id);
}
